using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Forms.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Forms.Commands.UpdateForm;

/// <summary>
/// 更新表單命令處理器
/// </summary>
public class UpdateFormCommandHandler : IRequestHandler<UpdateFormCommand, FormDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FormDto> Handle(UpdateFormCommand request, CancellationToken cancellationToken)
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

        var canEdit = request.IsSystemAdmin ||
                     (userMember != null &&
                      (userMember.Role == ProjectRole.Owner ||
                       userMember.Role == ProjectRole.Manager ||
                       form.CreatedById == request.CurrentUserId));

        if (!canEdit)
        {
            throw new UnauthorizedAccessException("您沒有權限編輯此表單");
        }

        // 如果表單已發布且有提交，需要增加版本號
        var shouldIncrementVersion = form.PublishedAt != null && form.Submissions.Any();

        // 更新表單資料
        form.Name = request.Name;
        form.Description = request.Description;
        form.Schema = request.Schema;
        form.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.AccessControl) &&
            Enum.TryParse<FormAccessControl>(request.AccessControl, true, out var accessControl))
        {
            form.AccessControl = accessControl;
        }

        if (request.IsActive.HasValue)
        {
            form.IsActive = request.IsActive.Value;
        }

        // 自動增加次版本號
        if (shouldIncrementVersion)
        {
            var versionParts = form.Version.Split('.');
            if (versionParts.Length >= 2 && int.TryParse(versionParts[1], out var minor))
            {
                form.Version = $"{versionParts[0]}.{minor + 1}";
            }
        }

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
