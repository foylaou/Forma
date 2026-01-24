using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Dashboard.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Dashboard.Queries.GetPendingTasks;

public class GetPendingTasksQueryHandler : IRequestHandler<GetPendingTasksQuery, List<PendingTaskDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPendingTasksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PendingTaskDto>> Handle(GetPendingTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = new List<PendingTaskDto>();

        // 取得使用者是 Manager 以上的專案
        var managerProjectIds = await _context.ProjectMembers
            .Where(pm => pm.UserId == request.CurrentUserId &&
                         pm.RemovedAt == null &&
                         pm.Role >= ProjectRole.Manager)
            .Select(pm => pm.ProjectId)
            .ToListAsync(cancellationToken);

        // 待審核的提交
        var pendingSubmissions = await _context.FormSubmissions
            .Include(s => s.Form)
            .Include(s => s.Project)
            .Where(s => managerProjectIds.Contains(s.ProjectId) &&
                        s.Status == SubmissionStatus.Submitted)
            .OrderByDescending(s => s.SubmittedAt)
            .Take(request.Limit)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var submission in pendingSubmissions)
        {
            tasks.Add(new PendingTaskDto
            {
                Type = "PendingReview",
                Title = $"審核提交 - {submission.Form.Name}",
                Description = $"來自 {submission.Project.Name} 的表單提交待審核",
                Link = $"/submissions/{submission.Id}",
                EntityId = submission.Id,
                EntityType = "Submission",
                ProjectName = submission.Project.Name,
                CreatedAt = submission.SubmittedAt,
                Priority = "Normal"
            });
        }

        return tasks.OrderByDescending(t => t.CreatedAt).Take(request.Limit).ToList();
    }
}
