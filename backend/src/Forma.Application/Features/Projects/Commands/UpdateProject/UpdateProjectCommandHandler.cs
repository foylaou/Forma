using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Projects.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Commands.UpdateProject;

/// <summary>
/// 更新專案命令處理器
/// </summary>
public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.CreatedBy)
            .Include(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(p => p.Forms)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException("找不到專案");
        }

        // 檢查權限：必須是專案 Manager/Owner 或系統管理員
        var userMember = project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId);

        var canEdit = request.IsSystemAdmin ||
                     (userMember != null &&
                      (userMember.Role == ProjectRole.Owner || userMember.Role == ProjectRole.Manager));

        if (!canEdit)
        {
            throw new UnauthorizedAccessException("您沒有權限編輯此專案");
        }

        // 更新專案資料
        project.Name = request.Name;
        project.Description = request.Description;
        project.Year = request.Year;
        project.Budget = request.Budget;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.UpdatedAt = DateTime.UtcNow;

        // 更新狀態（如果有提供）
        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
        {
            project.Status = status;
        }

        await _context.SaveChangesAsync(cancellationToken);

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
            CurrentUserRole = userMember?.Role.ToString()
        };
    }
}
