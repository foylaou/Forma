using Forma.Application.Common.Interfaces;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Commands.RemoveProjectMember;

/// <summary>
/// 移除專案成員命令處理器
/// </summary>
public class RemoveProjectMemberCommandHandler : IRequestHandler<RemoveProjectMemberCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public RemoveProjectMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(RemoveProjectMemberCommand request, CancellationToken cancellationToken)
    {
        // 查詢專案成員
        var projectMember = await _context.ProjectMembers
            .Include(m => m.Project)
                .ThenInclude(p => p.Members.Where(m => m.RemovedAt == null))
            .FirstOrDefaultAsync(m =>
                m.ProjectId == request.ProjectId &&
                m.UserId == request.UserId &&
                m.RemovedAt == null,
                cancellationToken);

        if (projectMember == null)
        {
            throw new KeyNotFoundException("找不到專案成員");
        }

        // 檢查權限
        var currentUserMember = projectMember.Project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId);

        // 成員可以自行退出專案
        var isSelfRemoval = request.UserId == request.CurrentUserId;

        // Manager/Owner 或系統管理員可以移除成員
        var canRemoveOthers = request.IsSystemAdmin ||
                             (currentUserMember != null &&
                              (currentUserMember.Role == ProjectRole.Owner ||
                               currentUserMember.Role == ProjectRole.Manager));

        if (!isSelfRemoval && !canRemoveOthers)
        {
            throw new UnauthorizedAccessException("您沒有權限移除此成員");
        }

        // Manager 不能移除 Owner 或其他 Manager
        if (!request.IsSystemAdmin &&
            currentUserMember != null &&
            currentUserMember.Role == ProjectRole.Manager &&
            projectMember.Role != ProjectRole.Member)
        {
            throw new UnauthorizedAccessException("管理者無法移除其他管理者或擁有者");
        }

        // 如果移除的是 Owner，需要確保至少還有一個 Owner
        if (projectMember.Role == ProjectRole.Owner)
        {
            var ownerCount = projectMember.Project.Members
                .Count(m => m.Role == ProjectRole.Owner);

            if (ownerCount <= 1)
            {
                throw new InvalidOperationException("專案至少需要一個擁有者，請先指派其他擁有者");
            }
        }

        // 軟刪除成員
        projectMember.RemovedAt = DateTime.UtcNow;
        projectMember.RemovedById = request.CurrentUserId;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
