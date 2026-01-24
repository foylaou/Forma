using Forma.Application.Features.Notifications.DTOs;
using MediatR;

namespace Forma.Application.Features.Notifications.Commands.UpdatePreferences;

public class UpdatePreferencesCommand : IRequest<NotificationPreferenceDto>
{
    public Guid CurrentUserId { get; set; }
    public bool EmailEnabled { get; set; }
    public bool InAppEnabled { get; set; }
    public bool NotifyOnFormSubmission { get; set; }
    public bool NotifyOnSubmissionReview { get; set; }
    public bool NotifyOnMemberChange { get; set; }
    public bool NotifyOnProjectUpdate { get; set; }
    public bool NotifyOnFormPublish { get; set; }
    public bool DailyDigestEmail { get; set; }
}
