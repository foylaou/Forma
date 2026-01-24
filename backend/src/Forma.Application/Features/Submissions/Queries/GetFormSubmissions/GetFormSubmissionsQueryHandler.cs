using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Submissions.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Submissions.Queries.GetFormSubmissions;

/// <summary>
/// 取得表單提交列表查詢處理器
/// </summary>
public class GetFormSubmissionsQueryHandler : IRequestHandler<GetFormSubmissionsQuery, PagedResult<SubmissionListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFormSubmissionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SubmissionListDto>> Handle(GetFormSubmissionsQuery request, CancellationToken cancellationToken)
    {
        // 驗證表單存在並檢查權限
        var form = await _context.Forms
            .Include(f => f.Project)
                .ThenInclude(p => p.Members.Where(m => m.RemovedAt == null))
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("找不到表單");
        }

        // 權限檢查：專案成員或系統管理員
        var isMember = form.Project.Members.Any(m => m.UserId == request.CurrentUserId);
        if (!request.IsSystemAdmin && !isMember)
        {
            throw new UnauthorizedAccessException();
        }

        var query = _context.FormSubmissions
            .Include(s => s.Form)
            .Include(s => s.SubmittedBy)
            .AsNoTracking()
            .Where(s => s.FormId == request.FormId);

        // 狀態篩選
        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<SubmissionStatus>(request.Status, true, out var status))
        {
            query = query.Where(s => s.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // 排序
        query = request.SortBy?.ToLower() switch
        {
            "status" => request.SortDescending
                ? query.OrderByDescending(s => s.Status)
                : query.OrderBy(s => s.Status),
            "submittedby" => request.SortDescending
                ? query.OrderByDescending(s => s.SubmittedBy != null ? s.SubmittedBy.Username : "")
                : query.OrderBy(s => s.SubmittedBy != null ? s.SubmittedBy.Username : ""),
            _ => request.SortDescending
                ? query.OrderByDescending(s => s.SubmittedAt)
                : query.OrderBy(s => s.SubmittedAt)
        };

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var submissions = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SubmissionListDto
            {
                Id = s.Id,
                FormId = s.FormId,
                FormName = s.Form.Name,
                SubmittedById = s.SubmittedById,
                SubmittedByUsername = s.SubmittedBy != null ? s.SubmittedBy.Username : null,
                SubmittedAt = s.SubmittedAt,
                Status = s.Status.ToString(),
                ReviewedAt = s.ReviewedAt
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<SubmissionListDto>
        {
            Items = submissions,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
