using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Forms.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Forms.Commands.UnpublishForm;

/// <summary>
/// 下架表單命令處理器
/// </summary>
public class UnpublishFormCommandHandler : IRequestHandler<UnpublishFormCommand, FormDto>
{
    private readonly IApplicationDbContext _context;

    public UnpublishFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FormDto> Handle(UnpublishFormCommand request, CancellationToken cancellationToken)
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

        // 檢查權限：必須是 Owner/Manager 或系統管理員
        var userMember = form.Project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId);

        var canUnpublish = request.IsSystemAdmin ||
                          (userMember != null &&
                           (userMember.Role == ProjectRole.Owner ||
                            userMember.Role == ProjectRole.Manager));

        if (!canUnpublish)
        {
            throw new UnauthorizedAccessException("您沒有權限下架此表單");
        }

        // 檢查表單是否已發布
        if (form.PublishedAt == null)
        {
            throw new InvalidOperationException("表單尚未發布");
        }

        // 下架表單
        form.PublishedAt = null;
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
