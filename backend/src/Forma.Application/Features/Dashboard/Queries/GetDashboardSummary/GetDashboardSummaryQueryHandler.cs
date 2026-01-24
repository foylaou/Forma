using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Dashboard.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// 取得儀表板摘要查詢處理器
/// </summary>
public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, DashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetDashboardSummaryQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> Handle(GetDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        // 取得使用者參與的專案
        var userProjectIds = await _context.ProjectMembers
            .Where(pm => pm.UserId == request.CurrentUserId && pm.RemovedAt == null)
            .Select(pm => pm.ProjectId)
            .ToListAsync(cancellationToken);

        var projects = await _context.Projects
            .Where(p => userProjectIds.Contains(p.Id))
            .Include(p => p.Organization)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var activeProjects = projects.Where(p => p.Status == ProjectStatus.Active).ToList();

        // 取得表單統計
        var forms = await _context.Forms
            .Where(f => userProjectIds.Contains(f.ProjectId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // 取得提交統計
        var submissions = await _context.FormSubmissions
            .Where(s => userProjectIds.Contains(s.ProjectId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // 取得未讀通知數
        var unreadNotifications = await _context.Notifications
            .CountAsync(n => n.UserId == request.CurrentUserId && !n.IsRead, cancellationToken);

        // 取得最近專案
        var recentProjects = new List<RecentProjectDto>();
        foreach (var project in projects.OrderByDescending(p => p.CreatedAt).Take(5))
        {
            var projectForms = forms.Where(f => f.ProjectId == project.Id).ToList();
            var projectSubmissions = submissions.Where(s => s.ProjectId == project.Id).ToList();
            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm => pm.ProjectId == project.Id && pm.UserId == request.CurrentUserId && pm.RemovedAt == null, cancellationToken);

            recentProjects.Add(new RecentProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                OrganizationName = project.Organization?.Name ?? "",
                Role = membership?.Role.ToString() ?? "",
                FormCount = projectForms.Count,
                PendingSubmissions = projectSubmissions.Count(s => s.Status == SubmissionStatus.Submitted),
                LastActivityAt = projectSubmissions.Any()
                    ? projectSubmissions.Max(s => s.SubmittedAt)
                    : project.CreatedAt
            });
        }

        return new DashboardSummaryDto
        {
            TotalProjects = projects.Count,
            ActiveProjects = activeProjects.Count,
            TotalForms = forms.Count,
            PublishedForms = forms.Count(f => f.PublishedAt != null),
            TotalSubmissions = submissions.Count,
            PendingReviewCount = submissions.Count(s => s.Status == SubmissionStatus.Submitted),
            UnreadNotifications = unreadNotifications,
            RecentProjects = recentProjects
        };
    }
}
