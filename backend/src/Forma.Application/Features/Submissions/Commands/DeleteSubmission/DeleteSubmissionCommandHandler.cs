using Forma.Application.Common.Interfaces;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Submissions.Commands.DeleteSubmission;

/// <summary>
/// 刪除表單提交命令處理器
/// </summary>
public class DeleteSubmissionCommandHandler : IRequestHandler<DeleteSubmissionCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteSubmissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _context.FormSubmissions
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId, cancellationToken);

        if (submission == null)
        {
            throw new KeyNotFoundException("找不到提交記錄");
        }

        // 權限檢查：提交者本人或系統管理員
        var isOwner = submission.SubmittedById == request.CurrentUserId;
        if (!request.IsSystemAdmin && !isOwner)
        {
            throw new UnauthorizedAccessException();
        }

        // 只有草稿狀態可以刪除（除非是系統管理員）
        if (!request.IsSystemAdmin && submission.Status != SubmissionStatus.Draft)
        {
            throw new InvalidOperationException("只能刪除草稿狀態的提交記錄");
        }

        _context.FormSubmissions.Remove(submission);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
