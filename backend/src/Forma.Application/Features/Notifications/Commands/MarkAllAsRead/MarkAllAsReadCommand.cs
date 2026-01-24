using MediatR;

namespace Forma.Application.Features.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadCommand : IRequest<int>
{
    public Guid CurrentUserId { get; set; }
}
