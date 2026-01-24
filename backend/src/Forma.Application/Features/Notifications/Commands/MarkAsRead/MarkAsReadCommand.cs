using MediatR;

namespace Forma.Application.Features.Notifications.Commands.MarkAsRead;

public class MarkAsReadCommand : IRequest<Unit>
{
    public Guid NotificationId { get; set; }
    public Guid CurrentUserId { get; set; }
}
