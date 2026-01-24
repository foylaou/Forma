using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Dashboard.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Dashboard.Queries.GetRecentActivities;

public class GetRecentActivitiesQueryHandler : IRequestHandler<GetRecentActivitiesQuery, List<RecentActivityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecentActivitiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecentActivityDto>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = new List<RecentActivityDto>();

        // 取得使用者參與的專案
        var userProjectIds = await _context.ProjectMembers
            .Where(pm => pm.UserId == request.CurrentUserId && pm.RemovedAt == null)
            .Select(pm => pm.ProjectId)
            .ToListAsync(cancellationToken);

        // 最近的提交
        var recentSubmissions = await _context.FormSubmissions
            .Include(s => s.Form)
            .Include(s => s.Project)
            .Include(s => s.SubmittedBy)
            .Where(s => userProjectIds.Contains(s.ProjectId))
            .OrderByDescending(s => s.SubmittedAt)
            .Take(10)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var submission in recentSubmissions)
        {
            activities.Add(new RecentActivityDto
            {
                Type = "Submission",
                Description = $"{submission.SubmittedBy?.Username ?? "匿名"} 提交了表單",
                ActorName = submission.SubmittedBy?.Username,
                ProjectName = submission.Project.Name,
                FormName = submission.Form.Name,
                EntityId = submission.Id,
                EntityType = "Submission",
                OccurredAt = submission.SubmittedAt
            });
        }

        // 最近發布的表單
        var recentPublishedForms = await _context.Forms
            .Include(f => f.Project)
            .Include(f => f.CreatedBy)
            .Where(f => userProjectIds.Contains(f.ProjectId) &&
                        f.PublishedAt != null)
            .OrderByDescending(f => f.PublishedAt)
            .Take(5)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var form in recentPublishedForms)
        {
            activities.Add(new RecentActivityDto
            {
                Type = "FormPublished",
                Description = $"{form.CreatedBy.Username} 發布了表單 {form.Name}",
                ActorName = form.CreatedBy.Username,
                ProjectName = form.Project.Name,
                FormName = form.Name,
                EntityId = form.Id,
                EntityType = "Form",
                OccurredAt = form.PublishedAt!.Value
            });
        }

        return activities.OrderByDescending(a => a.OccurredAt).Take(request.Limit).ToList();
    }
}
