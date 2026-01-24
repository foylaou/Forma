using Forma.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Notifications.Commands.MarkAllAsRead;

public class MarkAllAsReadCommandHandler : IRequestHandler<MarkAllAsReadCommand, int>
{
    private readonly IApplicationDbContext _context;

    public MarkAllAsReadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(MarkAllAsReadCommand request, CancellationToken cancellationToken)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == request.CurrentUserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return unreadNotifications.Count;
    }
}
