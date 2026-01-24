using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Notifications.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountQueryHandler : IRequestHandler<GetUnreadCountQuery, UnreadCountDto>
{
    private readonly IApplicationDbContext _context;

    public GetUnreadCountQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UnreadCountDto> Handle(GetUnreadCountQuery request, CancellationToken cancellationToken)
    {
        var count = await _context.Notifications
            .CountAsync(n => n.UserId == request.CurrentUserId && !n.IsRead, cancellationToken);

        return new UnreadCountDto { Count = count };
    }
}
