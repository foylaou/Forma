using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Templates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Templates.Queries.GetTemplateById;

/// <summary>
/// 取得範本詳情查詢處理器
/// </summary>
public class GetTemplateByIdQueryHandler : IRequestHandler<GetTemplateByIdQuery, TemplateDto>
{
    private readonly IApplicationDbContext _context;

    public GetTemplateByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateDto> Handle(GetTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _context.FormTemplates
            .Include(t => t.CreatedBy)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException("找不到範本");
        }

        // 權限檢查：公開範本、建立者或系統管理員
        var isOwner = template.CreatedById == request.CurrentUserId;
        if (!template.IsPublic && !isOwner && !request.IsSystemAdmin)
        {
            throw new UnauthorizedAccessException();
        }

        return new TemplateDto
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Category = template.Category,
            Schema = template.Schema,
            ThumbnailUrl = template.ThumbnailUrl,
            IsPublic = template.IsPublic,
            CreatedById = template.CreatedById,
            CreatedByUsername = template.CreatedBy.Username,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt,
            UsageCount = template.UsageCount,
            CanEdit = isOwner || request.IsSystemAdmin,
            CanDelete = isOwner || request.IsSystemAdmin
        };
    }
}
