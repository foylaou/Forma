using Forma.Application.Common.Interfaces;
using Forma.Domain.Entities;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Submissions.Commands.CreateSubmission;

/// <summary>
/// 建立表單提交命令處理器
/// </summary>
public class CreateSubmissionCommandHandler : IRequestHandler<CreateSubmissionCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateSubmissionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateSubmissionCommand request, CancellationToken cancellationToken)
    {
        // 取得表單
        var form = await _context.Forms
            .Include(f => f.Project)
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("找不到表單");
        }

        // 檢查表單是否已發布
        if (form.PublishedAt == null)
        {
            throw new InvalidOperationException("表單尚未發布，無法提交");
        }

        // 檢查表單是否啟用
        if (!form.IsActive)
        {
            throw new InvalidOperationException("表單已停用，無法提交");
        }

        var submission = new FormSubmission
        {
            Id = Guid.CreateVersion7(),
            FormId = form.Id,
            ProjectId = form.ProjectId,
            SubmittedById = request.CurrentUserId,
            SubmissionData = request.SubmissionData,
            FormVersion = form.Version,
            SubmittedAt = DateTime.UtcNow,
            Status = request.IsDraft ? SubmissionStatus.Draft : SubmissionStatus.Submitted,
            IpAddress = request.IpAddress
        };

        _context.FormSubmissions.Add(submission);
        await _context.SaveChangesAsync(cancellationToken);

        return submission.Id;
    }
}
