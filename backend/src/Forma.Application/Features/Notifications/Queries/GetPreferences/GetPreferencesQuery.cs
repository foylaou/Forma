using Forma.Application.Features.Notifications.DTOs;
using MediatR;

namespace Forma.Application.Features.Notifications.Queries.GetPreferences;

public class GetPreferencesQuery : IRequest<NotificationPreferenceDto>
{
    public Guid CurrentUserId { get; set; }
}
