using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Notifications.DTOs;
using Forma.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Notifications.Queries.GetPreferences;

public class GetPreferencesQueryHandler : IRequestHandler<GetPreferencesQuery, NotificationPreferenceDto>
{
    private readonly IApplicationDbContext _context;

    public GetPreferencesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreferenceDto> Handle(GetPreferencesQuery request, CancellationToken cancellationToken)
    {
        var preference = await _context.NotificationPreferences
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == request.CurrentUserId, cancellationToken);

        if (preference == null)
        {
            // 回傳預設值
            return new NotificationPreferenceDto
            {
                EmailEnabled = true,
                InAppEnabled = true,
                NotifyOnFormSubmission = true,
                NotifyOnSubmissionReview = true,
                NotifyOnMemberChange = true,
                NotifyOnProjectUpdate = true,
                NotifyOnFormPublish = true,
                DailyDigestEmail = false
            };
        }

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
