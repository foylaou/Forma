using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Dashboard.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Dashboard.Queries.GetPersonalStatistics;

public class GetPersonalStatisticsQueryHandler : IRequestHandler<GetPersonalStatisticsQuery, PersonalStatisticsDto>
{
    private readonly IApplicationDbContext _context;

    public GetPersonalStatisticsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PersonalStatisticsDto> Handle(GetPersonalStatisticsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonthStart = thisMonthStart.AddMonths(-1);
        var thirtyDaysAgo = now.AddDays(-30).Date;

        // 本月提交數
        var submissionsThisMonth = await _context.FormSubmissions
            .CountAsync(s => s.SubmittedById == request.CurrentUserId &&
                             s.SubmittedAt >= thisMonthStart, cancellationToken);

        // 上月提交數
        var submissionsLastMonth = await _context.FormSubmissions
            .CountAsync(s => s.SubmittedById == request.CurrentUserId &&
                             s.SubmittedAt >= lastMonthStart &&
                             s.SubmittedAt < thisMonthStart, cancellationToken);

        // 本月審核數
        var reviewedThisMonth = await _context.FormSubmissions
            .CountAsync(s => s.ReviewedById == request.CurrentUserId &&
                             s.ReviewedAt >= thisMonthStart, cancellationToken);

        // 本月建立表單數
        var formsCreatedThisMonth = await _context.Forms
            .CountAsync(f => f.CreatedById == request.CurrentUserId &&
                             f.CreatedAt >= thisMonthStart, cancellationToken);

        // 每日活動統計
        var dailySubmissions = await _context.FormSubmissions
            .Where(s => s.SubmittedById == request.CurrentUserId &&
                        s.SubmittedAt >= thirtyDaysAgo)
            .GroupBy(s => s.SubmittedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var dailyReviews = await _context.FormSubmissions
            .Where(s => s.ReviewedById == request.CurrentUserId &&
                        s.ReviewedAt >= thirtyDaysAgo)
            .GroupBy(s => s.ReviewedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var dailyActivities = new List<DailyActivityStat>();
        for (var date = thirtyDaysAgo; date <= now.Date; date = date.AddDays(1))
        {
            dailyActivities.Add(new DailyActivityStat
            {
                Date = date,
                Submissions = dailySubmissions.FirstOrDefault(d => d.Date == date)?.Count ?? 0,
                Reviews = dailyReviews.FirstOrDefault(d => d.Date == date)?.Count ?? 0
            });
        }

        return new PersonalStatisticsDto
        {
            SubmissionsThisMonth = submissionsThisMonth,
            SubmissionsLastMonth = submissionsLastMonth,
            ReviewedThisMonth = reviewedThisMonth,
            FormsCreatedThisMonth = formsCreatedThisMonth,
            AverageReviewTime = 0, // 可以後續實作
            DailyActivities = dailyActivities
        };
    }
}
