using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Templates.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Templates.Queries.GetTemplates;

/// <summary>
/// 取得範本列表查詢處理器
/// </summary>
public class GetTemplatesQueryHandler : IRequestHandler<GetTemplatesQuery, PagedResult<TemplateListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<TemplateListDto>> Handle(GetTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.FormTemplates
            .Include(t => t.CreatedBy)
            .AsNoTracking();

        // 篩選：公開範本或自己建立的
        if (!request.IsSystemAdmin)
        {
            if (request.OnlyMine)
            {
                query = query.Where(t => t.CreatedById == request.CurrentUserId);
            }
            else
            {
                query = query.Where(t => t.IsPublic || t.CreatedById == request.CurrentUserId);
            }
        }

        // 公開篩選
        if (request.IsPublic.HasValue)
        {
            query = query.Where(t => t.IsPublic == request.IsPublic.Value);
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                (t.Description != null && t.Description.ToLower().Contains(term)));
        }

        // 分類篩選
        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(t => t.Category == request.Category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(t => t.Name)
                : query.OrderBy(t => t.Name),
            "usagecount" => request.SortDescending
                ? query.OrderByDescending(t => t.UsageCount)
                : query.OrderBy(t => t.UsageCount),
            "category" => request.SortDescending
                ? query.OrderByDescending(t => t.Category)
                : query.OrderBy(t => t.Category),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var templates = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TemplateListDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Category = t.Category,
                ThumbnailUrl = t.ThumbnailUrl,
                IsPublic = t.IsPublic,
                CreatedByUsername = t.CreatedBy.Username,
                CreatedAt = t.CreatedAt,
                UsageCount = t.UsageCount
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<TemplateListDto>
        {
            Items = templates,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
