using Forma.Application.Common.Interfaces;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Commands.DeleteProject;

/// <summary>
/// 刪除專案命令處理器
/// </summary>
public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(p => p.Forms)
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException("找不到專案");
        }

        // 檢查權限：必須是專案 Owner 或系統管理員
        var userMember = project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId);

        var canDelete = request.IsSystemAdmin ||
                       (userMember != null && userMember.Role == ProjectRole.Owner);

        if (!canDelete)
        {
            throw new UnauthorizedAccessException("您沒有權限刪除此專案");
        }

        // 檢查專案是否有表單
        if (project.Forms.Any())
        {
            throw new InvalidOperationException("無法刪除含有表單的專案，請先刪除所有表單");
        }

        // 軟刪除所有成員關聯
        foreach (var member in project.Members)
        {
            member.RemovedAt = DateTime.UtcNow;
            member.RemovedById = request.CurrentUserId;
        }

        // 刪除專案
        _context.Projects.Remove(project);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
