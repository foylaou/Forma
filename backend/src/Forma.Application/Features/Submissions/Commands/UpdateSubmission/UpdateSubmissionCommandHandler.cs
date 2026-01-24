using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Submissions.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Submissions.Commands.UpdateSubmission;

/// <summary>
/// 更新表單提交命令處理器
/// </summary>
public class UpdateSubmissionCommandHandler : IRequestHandler<UpdateSubmissionCommand, SubmissionDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateSubmissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubmissionDto> Handle(UpdateSubmissionCommand request, CancellationToken cancellationToken)
    {
        var submission = await _context.FormSubmissions
            .Include(s => s.Form)
            .Include(s => s.Project)
            .Include(s => s.SubmittedBy)
            .Include(s => s.ReviewedBy)
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

        // 只有草稿或已提交狀態可以更新資料
        if (submission.Status != SubmissionStatus.Draft && submission.Status != SubmissionStatus.Submitted)
        {
            if (!request.IsSystemAdmin)
            {
                throw new InvalidOperationException("此提交記錄已審核，無法修改");
            }
        }

        submission.SubmissionData = request.SubmissionData;
        submission.UpdatedAt = DateTime.UtcNow;

        // 更新狀態
        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<SubmissionStatus>(request.Status, true, out var status))
        {
            // 審核操作
            if (status == SubmissionStatus.Approved || status == SubmissionStatus.Rejected)
            {
                submission.ReviewedById = request.CurrentUserId;
                submission.ReviewedAt = DateTime.UtcNow;
            }
            submission.Status = status;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new SubmissionDto
        {
            Id = submission.Id,
            FormId = submission.FormId,
            FormName = submission.Form.Name,
            ProjectId = submission.ProjectId,
            ProjectName = submission.Project.Name,
            SubmittedById = submission.SubmittedById,
            SubmittedByUsername = submission.SubmittedBy?.Username,
            SubmissionData = submission.SubmissionData,
            FormVersion = submission.FormVersion,
            SubmittedAt = submission.SubmittedAt,
            UpdatedAt = submission.UpdatedAt,
            Status = submission.Status.ToString(),
            ReviewedById = submission.ReviewedById,
            ReviewedByUsername = submission.ReviewedBy?.Username,
            ReviewedAt = submission.ReviewedAt,
            IpAddress = submission.IpAddress,
            CanEdit = isOwner || request.IsSystemAdmin,
            CanDelete = isOwner || request.IsSystemAdmin
        };
    }
}
