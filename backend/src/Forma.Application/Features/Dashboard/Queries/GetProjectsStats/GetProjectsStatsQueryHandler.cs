using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Dashboard.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Dashboard.Queries.GetProjectsStats;

public class GetProjectsStatsQueryHandler : IRequestHandler<GetProjectsStatsQuery, List<ProjectStatsDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectsStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProjectStatsDto>> Handle(GetProjectsStatsQuery request, CancellationToken cancellationToken)
    {
        var memberships = await _context.ProjectMembers
            .Include(pm => pm.Project)
                .ThenInclude(p => p.Organization)
            .Where(pm => pm.UserId == request.CurrentUserId && pm.RemovedAt == null)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var projectIds = memberships.Select(m => m.ProjectId).ToList();

        var forms = await _context.Forms
            .Where(f => projectIds.Contains(f.ProjectId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var submissions = await _context.FormSubmissions
            .Where(s => projectIds.Contains(s.ProjectId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new List<ProjectStatsDto>();
        foreach (var membership in memberships)
        {
            var projectForms = forms.Where(f => f.ProjectId == membership.ProjectId).ToList();
            var projectSubmissions = submissions.Where(s => s.ProjectId == membership.ProjectId).ToList();

            result.Add(new ProjectStatsDto
            {
                ProjectId = membership.ProjectId,
                ProjectName = membership.Project.Name,
                OrganizationName = membership.Project.Organization?.Name ?? "",
                Role = membership.Role.ToString(),
                TotalForms = projectForms.Count,
                PublishedForms = projectForms.Count(f => f.PublishedAt != null),
                TotalSubmissions = projectSubmissions.Count,
                PendingReviews = projectSubmissions.Count(s => s.Status == SubmissionStatus.Submitted),
                ApprovedSubmissions = projectSubmissions.Count(s => s.Status == SubmissionStatus.Approved),
                RejectedSubmissions = projectSubmissions.Count(s => s.Status == SubmissionStatus.Rejected),
                LastSubmissionAt = projectSubmissions.Any() ? projectSubmissions.Max(s => s.SubmittedAt) : null
            });
        }

        return result.OrderByDescending(p => p.LastSubmissionAt ?? DateTime.MinValue).ToList();
    }
}
