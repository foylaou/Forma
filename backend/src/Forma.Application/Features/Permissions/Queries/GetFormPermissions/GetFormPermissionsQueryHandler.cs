using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Permissions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Permissions.Queries.GetFormPermissions;

/// <summary>
/// 取得表單權限查詢處理器
/// </summary>
public class GetFormPermissionsQueryHandler : IRequestHandler<GetFormPermissionsQuery, FormPermissionSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetFormPermissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FormPermissionSummaryDto> Handle(GetFormPermissionsQuery request, CancellationToken cancellationToken)
    {
        var form = await _context.Forms
            .Include(f => f.Project)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("找不到表單");
        }

        // 權限檢查：專案成員（Manager以上）或系統管理員
        if (!request.IsSystemAdmin)
        {
            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm =>
                    pm.ProjectId == form.ProjectId &&
                    pm.UserId == request.CurrentUserId &&
                    pm.RemovedAt == null,
                    cancellationToken);

            if (membership == null || membership.Role < Domain.Enums.ProjectRole.Manager)
            {
                throw new UnauthorizedAccessException();
            }
        }

        var permissions = await _context.FormPermissions
            .Include(p => p.User)
            .Include(p => p.GrantedBy)
            .Where(p => p.FormId == request.FormId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return new FormPermissionSummaryDto
        {
            FormId = form.Id,
            FormName = form.Name,
            Permissions = permissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                FormId = p.FormId,
                UserId = p.UserId,
                Username = p.User?.Username,
                UserEmail = p.User?.Email,
                ProjectMemberRole = p.ProjectMemberRole?.ToString(),
                PermissionType = p.PermissionType.ToString(),
                GrantedById = p.GrantedById,
                GrantedByUsername = p.GrantedBy.Username,
                GrantedAt = p.GrantedAt
            }).ToList()
        };
    }
}
