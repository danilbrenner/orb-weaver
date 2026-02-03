using Confluent.Kafka;
using OrbWeaver.Application;
using OrbWeaver.Application.Abstractions;
using OrbWeaver.Application.Handler;

namespace OrbWeaver.Host.Consumer;

public class KafkaConsumerService(
    ILogger<KafkaConsumerService> logger,
    IUpdateHandler updateHandler,
    IConfiguration configuration)
    : BackgroundService
{
    private readonly IConfigurationSection _config = configuration.GetSection("KafkaConsumer");
    private IConsumer<string, string>? _consumer;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);


    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        var (config, topic, consumerTimeoutMs) = CreateConsumerConfig();

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => logger.LogError("Error: {Reason}", e.Reason))
            .SetPartitionsAssignedHandler((_, partitions) =>
            {
                logger.LogInformation("Assigned partitions: [{Partitions}]",
                    string.Join(", ", partitions));
            })
            .SetPartitionsRevokedHandler((_, partitions) =>
            {
                logger.LogInformation("Revoked partitions: [{Partitions}]",
                    string.Join(", ", partitions));
            })
            .Build();

        _consumer.Subscribe(topic);
        logger.LogInformation("Kafka consumer started, subscribed to topic: {Topic}", topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(consumerTimeoutMs));

                    if (consumeResult == null) continue;

                    logger.LogInformation(
                        "Processing message from topic {Topic}, partition {Partition}, offset {Offset}: Key = {Key}, Value = {Value}",
                        consumeResult.Topic,
                        consumeResult.Partition.Value,
                        consumeResult.Offset.Value,
                        consumeResult.Message.Key,
                        consumeResult.Message.Value);
                    
                    updateHandler
                        .Handle(consumeResult.Message.Key, consumeResult.Message.Value, consumeResult.Message.Timestamp.UtcDateTime, stoppingToken)
                        .GetAwaiter()
                        .GetResult();

                    _consumer.StoreOffset(consumeResult);
                    _consumer.Commit(consumeResult);

                    logger.LogInformation("Committed offset {Offset}", consumeResult.Offset.Value);
                }
                catch (ConsumeException e)
                {
                    logger.LogError(e, "Error consuming message: {Reason}", e.Error.Reason);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing message");
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Kafka consumer is shutting down");
        }
        finally
        {
            _consumer.Close();
            logger.LogInformation("Kafka consumer closed");
        }
    }

    private (ConsumerConfig, string, int) CreateConsumerConfig()
    {
        var bootstrapServers =
            _config["BootstrapServers"]
            ?? throw new InvalidOperationException("KafkaConsumer:BootstrapServers is required");
        var groupId =
            _config["GroupId"]
            ?? throw new InvalidOperationException("KafkaConsumer:GroupId is required");
        var topic =
            _config["Topic"]
            ?? throw new InvalidOperationException("KafkaConsumer:Topic is required");
        var consumerTimeoutMs = _config.GetValue("ConsumerTimeoutMs", 100);

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        var securityProtocol = _config["SecurityProtocol"];
        if (!string.IsNullOrEmpty(securityProtocol))
        {
            if (Enum.TryParse<SecurityProtocol>(securityProtocol, true, out var securityProtocolEnum))
            {
                config.SecurityProtocol = securityProtocolEnum;
                logger.LogInformation("Security protocol set to: {SecurityProtocol}", securityProtocolEnum);
            }
        }

        var saslMechanism = _config["SaslMechanism"];
        if (!string.IsNullOrEmpty(saslMechanism))
        {
            if (Enum.TryParse<SaslMechanism>(saslMechanism, true, out var saslMechanismEnum))
            {
                config.SaslMechanism = saslMechanismEnum;
                config.SaslUsername = _config["SaslUsername"];
                config.SaslPassword = _config["SaslPassword"];
                logger.LogInformation("SASL authentication enabled with mechanism: {SaslMechanism}", saslMechanismEnum);
            }
        }

        // Configure SSL if provided
        var sslCaLocation = _config["SslCaLocation"];
        if (!string.IsNullOrEmpty(sslCaLocation))
        {
            config.SslCaLocation = sslCaLocation;
            logger.LogInformation("SSL CA certificate location set");
        }

        var sslCertificateLocation = _config["SslCertificateLocation"];
        if (!string.IsNullOrEmpty(sslCertificateLocation))
        {
            config.SslCertificateLocation = sslCertificateLocation;
            config.SslKeyLocation = _config["SslKeyLocation"];
            config.SslKeyPassword = _config["SslKeyPassword"];
            logger.LogInformation("SSL client certificate configured");
        }

        return (config, topic, consumerTimeoutMs);
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}