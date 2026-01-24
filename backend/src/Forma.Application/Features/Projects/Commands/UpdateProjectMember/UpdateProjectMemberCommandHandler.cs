using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Projects.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Commands.UpdateProjectMember;

/// <summary>
/// 更新專案成員命令處理器
/// </summary>
public class UpdateProjectMemberCommandHandler : IRequestHandler<UpdateProjectMemberCommand, ProjectMemberDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateProjectMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectMemberDto> Handle(UpdateProjectMemberCommand request, CancellationToken cancellationToken)
    {
        // 查詢專案成員
        var projectMember = await _context.ProjectMembers
            .Include(m => m.Project)
                .ThenInclude(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(m => m.User)
            .Include(m => m.AddedBy)
            .FirstOrDefaultAsync(m =>
                m.ProjectId == request.ProjectId &&
                m.UserId == request.UserId &&
                m.RemovedAt == null,
                cancellationToken);

        if (projectMember == null)
        {
            throw new KeyNotFoundException("找不到專案成員");
        }

        // 檢查權限：必須是專案 Owner 或系統管理員
        var currentUserMember = projectMember.Project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId);

        var isOwnerOrAdmin = request.IsSystemAdmin ||
                            (currentUserMember != null && currentUserMember.Role == ProjectRole.Owner);

        if (!isOwnerOrAdmin)
        {
            throw new UnauthorizedAccessException("只有專案擁有者或系統管理員可以變更成員角色");
        }

        // 不能修改自己的角色
        if (request.UserId == request.CurrentUserId)
        {
            throw new InvalidOperationException("無法修改自己的角色");
        }

        // 解析新角色
        var newRole = Enum.Parse<ProjectRole>(request.Role, true);

        // 如果將現有 Owner 降級，需要確保至少還有一個 Owner
        if (projectMember.Role == ProjectRole.Owner && newRole != ProjectRole.Owner)
        {
            var ownerCount = projectMember.Project.Members
                .Count(m => m.Role == ProjectRole.Owner);

            if (ownerCount <= 1)
            {
                throw new InvalidOperationException("專案至少需要一個擁有者");
            }
        }

        // 更新角色
        projectMember.Role = newRole;

        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectMemberDto
        {
            UserId = projectMember.UserId,
            Username = projectMember.User.Username,
            Email = projectMember.User.Email,
            Department = projectMember.User.Department,
            JobTitle = projectMember.User.JobTitle,
            Role = projectMember.Role.ToString(),
            AddedAt = projectMember.AddedAt,
            AddedById = projectMember.AddedById,
            AddedByUsername = projectMember.AddedBy.Username
        };
    }
}
