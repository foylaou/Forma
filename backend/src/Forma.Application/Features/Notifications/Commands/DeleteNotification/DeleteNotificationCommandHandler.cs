using Forma.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Notifications.Commands.DeleteNotification;

public class DeleteNotificationCommandHandler : IRequestHandler<DeleteNotificationCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteNotificationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == request.CurrentUserId, cancellationToken);

        if (notification == null)
        {
            throw new KeyNotFoundException("找不到通知");
        }

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
