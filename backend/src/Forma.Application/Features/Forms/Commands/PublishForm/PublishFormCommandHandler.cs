using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Forms.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Forms.Commands.PublishForm;

/// <summary>
/// 發布表單命令處理器
/// </summary>
public class PublishFormCommandHandler : IRequestHandler<PublishFormCommand, FormDto>
{
    private readonly IApplicationDbContext _context;

    public PublishFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FormDto> Handle(PublishFormCommand request, CancellationToken cancellationToken)
    {
        var form = await _context.Forms
            .Include(f => f.Project)
                .ThenInclude(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(f => f.CreatedBy)
            .Include(f => f.Template)
            .Include(f => f.Submissions)
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("找不到表單");
        }

        // 檢查權限：必須是 Owner/Manager 或表單建立者或系統管理員
        var userMember = form.Project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId);

        var canPublish = request.IsSystemAdmin ||
                        (userMember != null &&
                         (userMember.Role == ProjectRole.Owner ||
                          userMember.Role == ProjectRole.Manager ||
                          form.CreatedById == request.CurrentUserId));

        if (!canPublish)
        {
            throw new UnauthorizedAccessException("您沒有權限發布此表單");
        }

        // 檢查表單是否已發布
        if (form.PublishedAt != null)
        {
            throw new InvalidOperationException("表單已經發布");
        }

        // 發布表單
        form.PublishedAt = DateTime.UtcNow;
        form.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new FormDto
        {
            Id = form.Id,
            ProjectId = form.ProjectId,
            ProjectName = form.Project.Name,
            Name = form.Name,
            Description = form.Description,
            Schema = form.Schema,
            TemplateId = form.TemplateId,
            TemplateName = form.Template?.Name,
            CreatedById = form.CreatedById,
            CreatedByUsername = form.CreatedBy.Username,
            CreatedAt = form.CreatedAt,
            UpdatedAt = form.UpdatedAt,
            PublishedAt = form.PublishedAt,
            IsActive = form.IsActive,
            Version = form.Version,
            AccessControl = form.AccessControl.ToString(),
            SubmissionCount = form.Submissions.Count,
            CanEdit = true,
            CanDelete = userMember?.Role == ProjectRole.Owner ||
                       userMember?.Role == ProjectRole.Manager ||
                       request.IsSystemAdmin
        };
    }
}
