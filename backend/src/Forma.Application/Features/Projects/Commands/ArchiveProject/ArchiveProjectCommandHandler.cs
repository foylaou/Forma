using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Projects.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Commands.ArchiveProject;

/// <summary>
/// 封存專案命令處理器
/// </summary>
public class ArchiveProjectCommandHandler : IRequestHandler<ArchiveProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _context;

    public ArchiveProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto> Handle(ArchiveProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(p => p.Forms)
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException("找不到專案");
        }

        // 權限檢查：必須是專案擁有者或系統管理員
        var currentMember = project.Members.FirstOrDefault(m => m.UserId == request.CurrentUserId);
        var isOwner = currentMember?.Role == ProjectRole.Owner;

        if (!request.IsSystemAdmin && !isOwner)
        {
            throw new UnauthorizedAccessException("只有專案擁有者或系統管理員可以封存專案");
        }

        // 檢查專案狀態
        if (project.Status == ProjectStatus.Archived)
        {
            throw new InvalidOperationException("專案已經封存");
        }

        // 封存專案
        project.Status = ProjectStatus.Archived;
        project.UpdatedAt = DateTime.UtcNow;

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
            CurrentUserRole = currentMember?.Role.ToString()
        };
    }
}
