using Forma.Application.Features.Notifications.DTOs;
using MediatR;

namespace Forma.Application.Features.Notifications.Queries.GetUnreadCount;

public class GetUnreadCountQuery : IRequest<UnreadCountDto>
{
    public Guid CurrentUserId { get; set; }
}
