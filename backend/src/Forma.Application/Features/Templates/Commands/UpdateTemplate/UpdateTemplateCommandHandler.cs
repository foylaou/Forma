using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Templates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Templates.Commands.UpdateTemplate;

/// <summary>
/// 更新範本命令處理器
/// </summary>
public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, TemplateDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TemplateDto> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.FormTemplates
            .Include(t => t.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException("找不到範本");
        }

        // 權限檢查：建立者或系統管理員
        if (!request.IsSystemAdmin && template.CreatedById != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException();
        }

        template.Name = request.Name;
        template.Description = request.Description;
        template.Category = request.Category;
        template.Schema = request.Schema;
        template.ThumbnailUrl = request.ThumbnailUrl;

        if (request.IsPublic.HasValue)
        {
            template.IsPublic = request.IsPublic.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

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
            CanEdit = true,
            CanDelete = true
        };
    }
}
