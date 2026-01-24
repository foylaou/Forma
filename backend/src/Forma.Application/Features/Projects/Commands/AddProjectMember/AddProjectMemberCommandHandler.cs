using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Projects.DTOs;
using Forma.Domain.Entities;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Commands.AddProjectMember;

/// <summary>
/// 新增專案成員命令處理器
/// </summary>
public class AddProjectMemberCommandHandler : IRequestHandler<AddProjectMemberCommand, ProjectMemberDto>
{
    private readonly IApplicationDbContext _context;

    public AddProjectMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectMemberDto> Handle(AddProjectMemberCommand request, CancellationToken cancellationToken)
    {
        // 檢查專案是否存在
        var project = await _context.Projects
            .Include(p => p.Members.Where(m => m.RemovedAt == null))
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException("找不到專案");
        }

        // 檢查權限：必須是專案 Manager/Owner 或系統管理員
        var currentUserMember = project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId);

        var canManageMembers = request.IsSystemAdmin ||
                              (currentUserMember != null &&
                               (currentUserMember.Role == ProjectRole.Owner ||
                                currentUserMember.Role == ProjectRole.Manager));

        if (!canManageMembers)
        {
            throw new UnauthorizedAccessException("您沒有權限管理此專案的成員");
        }

        // 檢查要新增的使用者是否存在
        var userToAdd = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (userToAdd == null)
        {
            throw new KeyNotFoundException("找不到使用者");
        }

        // 檢查使用者是否已是專案成員
        var existingMember = project.Members
            .FirstOrDefault(m => m.UserId == request.UserId);

        if (existingMember != null)
        {
            throw new InvalidOperationException("該使用者已是專案成員");
        }

        // 解析角色
        var role = Enum.Parse<ProjectRole>(request.Role, true);

        // 只有 Owner 或系統管理員可以新增 Owner/Manager
        if (role == ProjectRole.Owner || role == ProjectRole.Manager)
        {
            var isOwnerOrAdmin = request.IsSystemAdmin ||
                                (currentUserMember != null && currentUserMember.Role == ProjectRole.Owner);

            if (!isOwnerOrAdmin)
            {
                throw new UnauthorizedAccessException("只有專案擁有者或系統管理員可以新增管理者角色");
            }
        }

        // 建立專案成員
        var projectMember = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            UserId = request.UserId,
            Role = role,
            AddedById = request.CurrentUserId,
            AddedAt = DateTime.UtcNow
        };

        _context.ProjectMembers.Add(projectMember);
        await _context.SaveChangesAsync(cancellationToken);

        // 載入關聯資料
        var addedBy = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.CurrentUserId, cancellationToken);

        return new ProjectMemberDto
        {
            UserId = userToAdd.Id,
            Username = userToAdd.Username,
            Email = userToAdd.Email,
            Department = userToAdd.Department,
            JobTitle = userToAdd.JobTitle,
            Role = projectMember.Role.ToString(),
            AddedAt = projectMember.AddedAt,
            AddedById = projectMember.AddedById,
            AddedByUsername = addedBy?.Username ?? string.Empty
        };
    }
}
