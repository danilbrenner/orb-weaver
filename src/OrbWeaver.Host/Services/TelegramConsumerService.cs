using Microsoft.Extensions.Options;
using OrbWeaver.Infrastructure.Notification;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OrbWeaver.Host.Services;

public class TelegramConsumerService(
    ILogger<TelegramConsumerService> logger,
    IOptions<TelegramOptions> options)
    : BackgroundService
{
    private readonly TelegramBotClient _botClient = new(options.Value.BotToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting Telegram Consumer Service");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = [UpdateType.Message]
        };

        try
        {
            await _botClient.ReceiveAsync(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in Telegram Consumer Service");
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
        CancellationToken cancellationToken)
    {
        try
        {
            if (update.Message is not { } message)
                return;

            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            logger.LogInformation("Received message '{MessageText}' from chat {ChatId}", messageText, chatId);

            if (!messageText.StartsWith("/start"))
                return;
            
            await botClient.SendMessage(
                chatId: chatId,
                text: $"🥷 Hello {message.Chat.Username ?? "@Username"}! Welcome to the chat id: `{chatId}`",
                parseMode: ParseMode.Markdown,
                cancellationToken: cancellationToken);

            logger.LogInformation("Sent chat ID {ChatId} in response to /start command", chatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling update");
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Error occurred while receiving Telegram updates");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping Telegram Bot Listener");
        await base.StopAsync(cancellationToken);
    }
}