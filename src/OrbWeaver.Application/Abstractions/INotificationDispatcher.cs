using OrbWeaver.Domain;

namespace OrbWeaver.Application.Abstractions;

public interface INotificationDispatcher
{
    Task SendNotification(Notification notification, CancellationToken cancellationToken = default);
}