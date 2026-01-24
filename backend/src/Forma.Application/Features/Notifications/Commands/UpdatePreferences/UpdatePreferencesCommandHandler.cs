using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Notifications.DTOs;
using Forma.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Notifications.Commands.UpdatePreferences;

public class UpdatePreferencesCommandHandler : IRequestHandler<UpdatePreferencesCommand, NotificationPreferenceDto>
{
    private readonly IApplicationDbContext _context;

    public UpdatePreferencesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreferenceDto> Handle(UpdatePreferencesCommand request, CancellationToken cancellationToken)
    {
        var preference = await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == request.CurrentUserId, cancellationToken);

        if (preference == null)
        {
            preference = new NotificationPreference
            {
                Id = Guid.CreateVersion7(),
                UserId = request.CurrentUserId
            };
            _context.NotificationPreferences.Add(preference);
        }

        preference.EmailEnabled = request.EmailEnabled;
        preference.InAppEnabled = request.InAppEnabled;
        preference.NotifyOnFormSubmission = request.NotifyOnFormSubmission;
        preference.NotifyOnSubmissionReview = request.NotifyOnSubmissionReview;
        preference.NotifyOnMemberChange = request.NotifyOnMemberChange;
        preference.NotifyOnProjectUpdate = request.NotifyOnProjectUpdate;
        preference.NotifyOnFormPublish = request.NotifyOnFormPublish;
        preference.DailyDigestEmail = request.DailyDigestEmail;
        preference.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new NotificationPreferenceDto
        {
            EmailEnabled = preference.EmailEnabled,
            InAppEnabled = preference.InAppEnabled,
            NotifyOnFormSubmission = preference.NotifyOnFormSubmission,
            NotifyOnSubmissionReview = preference.NotifyOnSubmissionReview,
            NotifyOnMemberChange = preference.NotifyOnMemberChange,
            NotifyOnProjectUpdate = preference.NotifyOnProjectUpdate,
            NotifyOnFormPublish = preference.NotifyOnFormPublish,
            DailyDigestEmail = preference.DailyDigestEmail
        };
    }
}
