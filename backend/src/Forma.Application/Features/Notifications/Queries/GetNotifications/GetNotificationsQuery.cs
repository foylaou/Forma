using Forma.Application.Common.Models;
using Forma.Application.Features.Notifications.DTOs;
using MediatR;

namespace Forma.Application.Features.Notifications.Queries.GetNotifications;

public class GetNotificationsQuery : IRequest<PagedResult<NotificationDto>>
{
    public Guid CurrentUserId { get; set; }
    public bool? IsRead { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
