using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Forms.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Forms.Queries.GetFormById;

/// <summary>
/// 取得表單詳情查詢處理器
/// </summary>
public class GetFormByIdQueryHandler : IRequestHandler<GetFormByIdQuery, FormDto>
{
    private readonly IApplicationDbContext _context;

    public GetFormByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FormDto> Handle(GetFormByIdQuery request, CancellationToken cancellationToken)
    {
        var form = await _context.Forms
            .Include(f => f.Project)
                .ThenInclude(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(f => f.CreatedBy)
            .Include(f => f.Template)
            .Include(f => f.Submissions)
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("找不到表單");
        }

        // 檢查權限
        var userMember = form.Project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId);
        var isMember = userMember != null;

        if (!isMember && !request.IsSystemAdmin)
        {
            throw new UnauthorizedAccessException("您沒有權限存取此表單");
        }

        // 判斷編輯和刪除權限
        var canEdit = request.IsSystemAdmin ||
                     (userMember != null &&
                      (userMember.Role == ProjectRole.Owner ||
                       userMember.Role == ProjectRole.Manager ||
                       form.CreatedById == request.CurrentUserId));

        var canDelete = request.IsSystemAdmin ||
                       (userMember != null &&
                        (userMember.Role == ProjectRole.Owner ||
                         userMember.Role == ProjectRole.Manager));

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
            CanEdit = canEdit,
            CanDelete = canDelete
        };
    }
}
