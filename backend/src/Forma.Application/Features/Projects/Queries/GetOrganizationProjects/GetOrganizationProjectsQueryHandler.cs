using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Projects.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Queries.GetOrganizationProjects;

/// <summary>
/// 取得組織下的專案查詢處理器
/// </summary>
public class GetOrganizationProjectsQueryHandler : IRequestHandler<GetOrganizationProjectsQuery, PagedResult<ProjectListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrganizationProjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProjectListDto>> Handle(GetOrganizationProjectsQuery request, CancellationToken cancellationToken)
    {
        // 驗證組織存在
        var organizationExists = await _context.Organizations
            .AnyAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (!organizationExists)
        {
            throw new KeyNotFoundException("找不到組織");
        }

        var query = _context.Projects
            .Include(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(p => p.Forms)
            .AsNoTracking()
            .Where(p => p.OrganizationId == request.OrganizationId);

        // 非系統管理員只能看到自己參與的專案
        if (!request.IsSystemAdmin)
        {
            query = query.Where(p => p.Members.Any(m =>
                m.UserId == request.CurrentUserId && m.RemovedAt == null));
        }

        // 搜尋
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Code.ToLower().Contains(term) ||
                (p.Description != null && p.Description.ToLower().Contains(term)));
        }

        // 年度篩選
        if (request.Year.HasValue)
        {
            query = query.Where(p => p.Year == request.Year.Value);
        }

        // 狀態篩選
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<ProjectStatus>(request.Status, true, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "code" => request.SortDescending ? query.OrderByDescending(p => p.Code) : query.OrderBy(p => p.Code),
            "year" => request.SortDescending ? query.OrderByDescending(p => p.Year) : query.OrderBy(p => p.Year),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var projects = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProjectListDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                Year = p.Year,
                Status = p.Status.ToString(),
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                MemberCount = p.Members.Count(m => m.RemovedAt == null),
                FormCount = p.Forms.Count,
                CurrentUserRole = p.Members
                    .Where(m => m.UserId == request.CurrentUserId && m.RemovedAt == null)
                    .Select(m => m.Role.ToString())
                    .FirstOrDefault(),
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectListDto>
        {
            Items = projects,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
