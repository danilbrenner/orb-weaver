using System.Collections.Immutable;
using OrbWeaver.Domain;

namespace OrbWeaver.Handler.Abstractions;

public interface INotificationDispatcher
{
    Task Dispatch(ImmutableList<Notification> notificationActions, CancellationToken cancellationToken);
}