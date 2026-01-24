using Forma.Application.Common.Interfaces;
using Forma.Domain.Entities;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Permissions.Commands.GrantPermission;

/// <summary>
/// 授予權限命令處理器
/// </summary>
public class GrantPermissionCommandHandler : IRequestHandler<GrantPermissionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public GrantPermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(GrantPermissionCommand request, CancellationToken cancellationToken)
    {
        var form = await _context.Forms
            .Include(f => f.Project)
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("找不到表單");
        }

        // 權限檢查
        if (!request.IsSystemAdmin)
        {
            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm =>
                    pm.ProjectId == form.ProjectId &&
                    pm.UserId == request.CurrentUserId &&
                    pm.RemovedAt == null,
                    cancellationToken);

            if (membership == null || membership.Role < ProjectRole.Manager)
            {
                throw new UnauthorizedAccessException();
            }
        }

        // 驗證權限類型
        if (!Enum.TryParse<PermissionType>(request.PermissionType, true, out var permissionType))
        {
            throw new InvalidOperationException($"無效的權限類型: {request.PermissionType}");
        }

        // 驗證角色（如果有指定）
        ProjectRole? projectRole = null;
        if (!string.IsNullOrEmpty(request.ProjectMemberRole))
        {
            if (!Enum.TryParse<ProjectRole>(request.ProjectMemberRole, true, out var role))
            {
                throw new InvalidOperationException($"無效的專案角色: {request.ProjectMemberRole}");
            }
            projectRole = role;
        }

        // 必須指定使用者或角色
        if (request.UserId == null && projectRole == null)
        {
            throw new InvalidOperationException("必須指定使用者或專案角色");
        }

        // 檢查是否已存在相同權限
        var existingPermission = await _context.FormPermissions
            .FirstOrDefaultAsync(p =>
                p.FormId == request.FormId &&
                p.UserId == request.UserId &&
                p.ProjectMemberRole == projectRole &&
                p.PermissionType == permissionType,
                cancellationToken);

        if (existingPermission != null)
        {
            throw new InvalidOperationException("此權限已存在");
        }

        // 如果指定使用者，驗證使用者存在
        if (request.UserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId.Value, cancellationToken);
            if (!userExists)
            {
                throw new KeyNotFoundException("找不到使用者");
            }
        }

        var permission = new FormPermission
        {
            Id = Guid.CreateVersion7(),
            FormId = request.FormId,
            UserId = request.UserId,
            ProjectMemberRole = projectRole,
            PermissionType = permissionType,
            GrantedById = request.CurrentUserId,
            GrantedAt = DateTime.UtcNow
        };

        _context.FormPermissions.Add(permission);
        await _context.SaveChangesAsync(cancellationToken);

        return permission.Id;
    }
}
