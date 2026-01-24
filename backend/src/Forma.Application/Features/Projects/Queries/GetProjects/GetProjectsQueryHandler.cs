using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Projects.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Queries.GetProjects;

/// <summary>
/// 取得專案列表查詢處理器
/// </summary>
public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, PagedResult<ProjectListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProjectsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ProjectListDto>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Projects
            .Include(p => p.Members.Where(m => m.RemovedAt == null))
            .Include(p => p.Forms)
            .AsNoTracking();

        // 非系統管理員只能看到自己參與的專案
        if (!request.IsSystemAdmin || request.OnlyMyProjects)
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

        // 計算總數
        var totalCount = await query.CountAsync(cancellationToken);

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDescending
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),
            "code" => request.SortDescending
                ? query.OrderByDescending(p => p.Code)
                : query.OrderBy(p => p.Code),
            "year" => request.SortDescending
                ? query.OrderByDescending(p => p.Year)
                : query.OrderBy(p => p.Year),
            "status" => request.SortDescending
                ? query.OrderByDescending(p => p.Status)
                : query.OrderBy(p => p.Status),
            "startdate" => request.SortDescending
                ? query.OrderByDescending(p => p.StartDate)
                : query.OrderBy(p => p.StartDate),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        // 分頁
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
