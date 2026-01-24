using MediatR;

namespace Forma.Application.Features.Notifications.Commands.DeleteNotification;

public class DeleteNotificationCommand : IRequest<Unit>
{
    public Guid NotificationId { get; set; }
    public Guid CurrentUserId { get; set; }
}
