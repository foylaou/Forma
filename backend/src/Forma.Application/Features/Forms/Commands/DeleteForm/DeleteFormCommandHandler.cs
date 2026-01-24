using Forma.Application.Common.Interfaces;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Forms.Commands.DeleteForm;

/// <summary>
/// 刪除表單命令處理器
/// </summary>
public class DeleteFormCommandHandler : IRequestHandler<DeleteFormCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteFormCommand request, CancellationToken cancellationToken)
    {
        var form = await _context.Forms
            .Include(f => f.Project)
                .ThenInclude(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(f => f.Submissions)
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("找不到表單");
        }

        // 檢查權限：必須是 Owner/Manager 或系統管理員
        var userMember = form.Project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId);

        var canDelete = request.IsSystemAdmin ||
                       (userMember != null &&
                        (userMember.Role == ProjectRole.Owner ||
                         userMember.Role == ProjectRole.Manager));

        if (!canDelete)
        {
            throw new UnauthorizedAccessException("您沒有權限刪除此表單");
        }

        // 檢查是否有提交資料
        if (form.Submissions.Any())
        {
            throw new InvalidOperationException("無法刪除已有提交資料的表單，請先刪除所有提交資料或改為停用表單");
        }

        _context.Forms.Remove(form);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
