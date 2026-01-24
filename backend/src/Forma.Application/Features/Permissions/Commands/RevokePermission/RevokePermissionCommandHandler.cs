using Forma.Application.Common.Interfaces;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Permissions.Commands.RevokePermission;

/// <summary>
/// 移除權限命令處理器
/// </summary>
public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public RevokePermissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await _context.FormPermissions
            .Include(p => p.Form)
            .FirstOrDefaultAsync(p => p.Id == request.PermissionId, cancellationToken);

        if (permission == null)
        {
            throw new KeyNotFoundException("找不到權限記錄");
        }

        // 權限檢查
        if (!request.IsSystemAdmin)
        {
            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm =>
                    pm.ProjectId == permission.Form.ProjectId &&
                    pm.UserId == request.CurrentUserId &&
                    pm.RemovedAt == null,
                    cancellationToken);

            if (membership == null || membership.Role < ProjectRole.Manager)
            {
                throw new UnauthorizedAccessException();
            }
        }

        _context.FormPermissions.Remove(permission);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
