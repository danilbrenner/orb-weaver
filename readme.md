## .env File Configuration

```hocon
POSTGRES_USER=<>
POSTGRES_PASSWORD=<>
POSTGRES_DB=<>
POSTGRES_PORT=<>

```

## Running the DB and Migrating

```bash
docker compose up -d
```

## Kafka Consumer Configuration Guide

### Configuration Keys

All configuration is under the `KafkaConsumer` section:

#### Required Settings

| Key | Type | Description |
|-----|------|-------------|
| `BootstrapServers` | string | Comma-separated list of Kafka broker addresses (e.g., `localhost:9092`) |
| `GroupId` | string | Consumer group ID for coordinating multiple consumers |
| `Topic` | string | Kafka topic to subscribe to |

#### Optional Settings

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `ConsumerTimeoutMs` | int | 100 | Timeout in milliseconds for consuming messages |

#### Authentication Settings

**Security Protocol**

| Key | Type | Values | Description |
|-----|------|--------|-------------|
| `SecurityProtocol` | string | `PLAINTEXT`, `SSL`, `SASL_PLAINTEXT`, `SASL_SSL` | Security protocol to use |

**SASL Authentication**

| Key | Type | Values | Description |
|-----|------|--------|-------------|
| `SaslMechanism` | string | `PLAIN`, `SCRAM-SHA-256`, `SCRAM-SHA-512`, `AWS_MSK_IAM` | SASL mechanism |
| `SaslUsername` | string | - | Username or API key for SASL authentication |
| `SaslPassword` | string | - | Password or API secret for SASL authentication |

**SSL/TLS Settings**

| Key | Type | Description |
|-----|------|-------------|
| `SslCaLocation` | string | Path to CA certificate file (e.g., `/path/to/ca-cert.pem`) |
| `SslCertificateLocation` | string | Path to client certificate file for mutual TLS |
| `SslKeyLocation` | string | Path to client private key file for mutual TLS |
| `SslKeyPassword` | string | Password for the client private key (if encrypted) |

### Common Scenarios

#### 1. Local Development (No Authentication)
```json
{
  "KafkaConsumer": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "orb-weaver-consumer",
    "Topic": "test-topic"
  }
}
```

#### 2. Confluent Cloud
```json
{
  "KafkaConsumer": {
    "BootstrapServers": "pkc-xxxxx.us-east-1.aws.confluent.cloud:9092",
    "GroupId": "orb-weaver-consumer",
    "Topic": "test-topic",
    "SecurityProtocol": "SASL_SSL",
    "SaslMechanism": "PLAIN",
    "SaslUsername": "YOUR_API_KEY",
    "SaslPassword": "YOUR_API_SECRET"
  }
}
```

#### 3. Production with SCRAM-SHA-256
```json
{
  "KafkaConsumer": {
    "BootstrapServers": "kafka-prod:9093",
    "GroupId": "orb-weaver-consumer",
    "Topic": "production-topic",
    "SecurityProtocol": "SASL_SSL",
    "SaslMechanism": "SCRAM-SHA-256",
    "SaslUsername": "prod-user",
    "SaslPassword": "secure-password"
  }
}
```

#### 4. Mutual TLS (Certificate-based Auth)
```json
{
  "KafkaConsumer": {
    "BootstrapServers": "kafka-secure:9093",
    "GroupId": "orb-weaver-consumer",
    "Topic": "secure-topic",
    "SecurityProtocol": "SSL",
    "SslCaLocation": "/certs/ca-cert.pem",
    "SslCertificateLocation": "/certs/client-cert.pem",
    "SslKeyLocation": "/certs/client-key.pem"
  }
}
```

### Security Best Practices

1. **Never commit credentials** to version control
2. **Use environment variables or secrets management** for production credentials
3. **Prefer SASL_SSL over SASL_PLAINTEXT** to encrypt credentials in transit
4. **Use SCRAM-SHA-256 or SCRAM-SHA-512** instead of PLAIN when possible
5. **Store certificates securely** and use proper file permissions (600 or 400)

### Using Environment Variables

You can override configuration using environment variables:

```bash
export KafkaConsumer__BootstrapServers="kafka-prod:9093"
export KafkaConsumer__SaslUsername="prod-user"
export KafkaConsumer__SaslPassword="secure-password"
```

Or in Docker Compose:
```yaml
environment:
  - KafkaConsumer__BootstrapServers=kafka-prod:9093
  - KafkaConsumer__SaslUsername=prod-user
  - KafkaConsumer__SaslPassword=${KAFKA_PASSWORD}
```

### Troubleshooting

#### Connection Issues
- Verify `BootstrapServers` is correct and reachable
- Check firewall rules and network connectivity
- Verify the correct port (usually 9092 for PLAINTEXT, 9093 for SSL/SASL)

#### Authentication Failures
- Double-check credentials (username/password or API key/secret)
- Verify `SecurityProtocol` and `SaslMechanism` match broker configuration
- Check certificate paths and permissions for SSL/TLS

#### SSL/TLS Issues
- Verify certificate files exist and are readable
- Check certificate format (PEM format required)
- Verify CA certificate matches the broker's certificate chain

