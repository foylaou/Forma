using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Forms.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Forms.Queries.GetForms;

/// <summary>
/// 取得專案表單列表查詢處理器
/// </summary>
public class GetFormsQueryHandler : IRequestHandler<GetFormsQuery, PagedResult<FormListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFormsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<FormListDto>> Handle(GetFormsQuery request, CancellationToken cancellationToken)
    {
        // 檢查專案是否存在且使用者有權限存取
        var project = await _context.Projects
            .Include(p => p.Members.Where(m => m.RemovedAt == null))
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException("找不到專案");
        }

        // 檢查權限：必須是專案成員或系統管理員
        var isMember = project.Members.Any(m => m.UserId == request.CurrentUserId);
        if (!isMember && !request.IsSystemAdmin)
        {
            throw new UnauthorizedAccessException("您沒有權限存取此專案的表單");
        }

        var query = _context.Forms
            .Include(f => f.CreatedBy)
            .Include(f => f.Submissions)
            .Where(f => f.ProjectId == request.ProjectId)
            .AsNoTracking();

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(f =>
                f.Name.ToLower().Contains(term) ||
                (f.Description != null && f.Description.ToLower().Contains(term)));
        }

        // 發布狀態篩選
        if (request.IsPublished.HasValue)
        {
            query = request.IsPublished.Value
                ? query.Where(f => f.PublishedAt != null)
                : query.Where(f => f.PublishedAt == null);
        }

        // 啟用狀態篩選
        if (request.IsActive.HasValue)
        {
            query = query.Where(f => f.IsActive == request.IsActive.Value);
        }

        // 計算總數
        var totalCount = await query.CountAsync(cancellationToken);

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(f => f.Name)
                : query.OrderBy(f => f.Name),
            "version" => request.SortDescending
                ? query.OrderByDescending(f => f.Version)
                : query.OrderBy(f => f.Version),
            "publishedat" => request.SortDescending
                ? query.OrderByDescending(f => f.PublishedAt)
                : query.OrderBy(f => f.PublishedAt),
            "submissioncount" => request.SortDescending
                ? query.OrderByDescending(f => f.Submissions.Count)
                : query.OrderBy(f => f.Submissions.Count),
            _ => query.OrderByDescending(f => f.CreatedAt)
        };

        // 分頁
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var forms = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new FormListDto
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                Version = f.Version,
                AccessControl = f.AccessControl.ToString(),
                IsActive = f.IsActive,
                IsPublished = f.PublishedAt != null,
                PublishedAt = f.PublishedAt,
                CreatedAt = f.CreatedAt,
                CreatedByUsername = f.CreatedBy.Username,
                SubmissionCount = f.Submissions.Count
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<FormListDto>
        {
            Items = forms,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
