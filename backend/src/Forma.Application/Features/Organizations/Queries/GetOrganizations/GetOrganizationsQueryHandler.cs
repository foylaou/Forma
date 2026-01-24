using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Organizations.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Organizations.Queries.GetOrganizations;

/// <summary>
/// 取得組織列表查詢處理器
/// </summary>
public class GetOrganizationsQueryHandler : IRequestHandler<GetOrganizationsQuery, PagedResult<OrganizationListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrganizationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<OrganizationListDto>> Handle(GetOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Organizations
            .Include(o => o.Projects)
            .AsNoTracking();

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(o =>
                o.Name.ToLower().Contains(term) ||
                o.Code.ToLower().Contains(term) ||
                (o.Description != null && o.Description.ToLower().Contains(term)));
        }

        // 類型篩選
        if (!string.IsNullOrWhiteSpace(request.Type) &&
            Enum.TryParse<OrganizationType>(request.Type, true, out var type))
        {
            query = query.Where(o => o.Type == type);
        }

        // 計算總數
        var totalCount = await query.CountAsync(cancellationToken);

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(o => o.Name)
                : query.OrderBy(o => o.Name),
            "code" => request.SortDescending
                ? query.OrderByDescending(o => o.Code)
                : query.OrderBy(o => o.Code),
            "type" => request.SortDescending
                ? query.OrderByDescending(o => o.Type)
                : query.OrderBy(o => o.Type),
            "projectcount" => request.SortDescending
                ? query.OrderByDescending(o => o.Projects.Count)
                : query.OrderBy(o => o.Projects.Count),
            _ => query.OrderBy(o => o.Name)
        };

        // 分頁
        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var organizations = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrganizationListDto
            {
                Id = o.Id,
                Name = o.Name,
                Code = o.Code,
                Type = o.Type.ToString(),
                ProjectCount = o.Projects.Count,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<OrganizationListDto>
        {
            Items = organizations,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
