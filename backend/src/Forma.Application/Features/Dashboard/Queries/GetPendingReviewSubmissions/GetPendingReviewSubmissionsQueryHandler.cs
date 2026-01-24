using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Dashboard.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Dashboard.Queries.GetPendingReviewSubmissions;

public class GetPendingReviewSubmissionsQueryHandler : IRequestHandler<GetPendingReviewSubmissionsQuery, PagedResult<PendingReviewSubmissionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingReviewSubmissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<PendingReviewSubmissionDto>> Handle(GetPendingReviewSubmissionsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Guid> projectIdsQuery;

        if (request.IsSystemAdmin)
        {
            projectIdsQuery = _context.Projects
                .Select(p => p.Id);
        }
        else
        {
            // 只取得 Manager 以上的專案
            projectIdsQuery = _context.ProjectMembers
                .Where(pm => pm.UserId == request.CurrentUserId &&
                             pm.RemovedAt == null &&
                             pm.Role >= ProjectRole.Manager)
                .Select(pm => pm.ProjectId);
        }

        var query = _context.FormSubmissions
            .Include(s => s.Form)
            .Include(s => s.Project)
            .Include(s => s.SubmittedBy)
            .Where(s => projectIdsQuery.Contains(s.ProjectId) &&
                        s.Status == SubmissionStatus.Submitted)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var submissions = await query
            .OrderByDescending(s => s.SubmittedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new PendingReviewSubmissionDto
            {
                Id = s.Id,
                FormId = s.FormId,
                FormName = s.Form.Name,
                ProjectId = s.ProjectId,
                ProjectName = s.Project.Name,
                SubmittedByUsername = s.SubmittedBy != null ? s.SubmittedBy.Username : null,
                SubmittedAt = s.SubmittedAt,
                Status = s.Status.ToString()
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<PendingReviewSubmissionDto>
        {
            Items = submissions,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
