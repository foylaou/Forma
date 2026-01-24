using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Submissions.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Submissions.Queries.GetSubmissionById;

/// <summary>
/// 取得提交詳情查詢處理器
/// </summary>
public class GetSubmissionByIdQueryHandler : IRequestHandler<GetSubmissionByIdQuery, SubmissionDto>
{
    private readonly IApplicationDbContext _context;

    public GetSubmissionByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubmissionDto> Handle(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var submission = await _context.FormSubmissions
            .Include(s => s.Form)
            .Include(s => s.Project)
                .ThenInclude(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(s => s.SubmittedBy)
            .Include(s => s.ReviewedBy)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == request.SubmissionId, cancellationToken);

        if (submission == null)
        {
            throw new KeyNotFoundException("找不到提交記錄");
        }

        // 權限檢查：提交者本人、專案成員或系統管理員
        var isOwner = submission.SubmittedById == request.CurrentUserId;
        var isMember = submission.Project.Members.Any(m => m.UserId == request.CurrentUserId);

        if (!request.IsSystemAdmin && !isOwner && !isMember)
        {
            throw new UnauthorizedAccessException();
        }

        var isManager = submission.Project.Members.Any(m =>
            m.UserId == request.CurrentUserId &&
            (m.Role == ProjectRole.Owner || m.Role == ProjectRole.Manager));

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
            CanDelete = (isOwner && submission.Status == SubmissionStatus.Draft) || request.IsSystemAdmin
        };
    }
}
