using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Projects.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Queries.GetProjectById;

/// <summary>
/// 取得專案詳情查詢處理器
/// </summary>
public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ProjectDto>
{
    private readonly IApplicationDbContext _context;

    public GetProjectByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.CreatedBy)
            .Include(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(p => p.Forms)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException("找不到專案");
        }

        // 檢查權限：必須是專案成員或系統管理員
        var isMember = project.Members.Any(m => m.UserId == request.CurrentUserId);
        if (!isMember && !request.IsSystemAdmin)
        {
            throw new UnauthorizedAccessException("您沒有權限查看此專案");
        }

        var currentUserRole = project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId)?
            .Role.ToString();

        return new ProjectDto
        {
            Id = project.Id,
            OrganizationId = project.OrganizationId,
            Name = project.Name,
            Code = project.Code,
            Description = project.Description,
            Year = project.Year,
            Budget = project.Budget,
            StartDate = project.StartDate,
            EndDate = project.EndDate,
            Status = project.Status.ToString(),
            CreatedById = project.CreatedById,
            CreatedByUsername = project.CreatedBy.Username,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            MemberCount = project.Members.Count,
            FormCount = project.Forms.Count,
            CurrentUserRole = currentUserRole
        };
    }
}
