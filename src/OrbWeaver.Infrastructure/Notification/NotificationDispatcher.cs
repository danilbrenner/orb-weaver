using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrbWeaver.Application.Abstractions;
using Telegram.Bot;

namespace OrbWeaver.Infrastructure.Notification;

public class NotificationDispatcher(ILogger<NotificationDispatcher> logger, IOptions<TelegramOptions> options) : INotificationDispatcher
{
    private readonly TelegramBotClient _botClient = new TelegramBotClient(options.Value.BotToken);
    private readonly TelegramOptions _options = options.Value;

    public async Task SendNotification(Domain.Notification notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await _botClient.SendMessage(
                chatId: _options.ChatId,
                text: FormatNotificationMessage(notification),
                cancellationToken: CancellationToken.None);
            
            logger.LogInformation(
                "Notification sent successfully for alert '{AlertName}' with status '{Status}'",
                notification.AlertName,
                notification.NewStatus);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, 
                "Failed to send notification for alert '{AlertName}' with status '{Status}'",
                notification.AlertName,
                notification.NewStatus);
            throw;
        }
    }
    
    private static string FormatNotificationMessage(Domain.Notification notification)
    {
        var statusEmoji = notification.NewStatus == Domain.AlertStatus.Active ? "🔥" : "✅";
        var statusText = notification.NewStatus == Domain.AlertStatus.Active ? "TRIGGERED" : "RESOLVED";
        
        return $"{statusEmoji} Alert: {notification.AlertName}: {statusText}";
    }
}

