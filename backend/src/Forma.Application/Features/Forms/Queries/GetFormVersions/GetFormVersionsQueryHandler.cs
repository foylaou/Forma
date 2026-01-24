using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Forms.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Forms.Queries.GetFormVersions;

/// <summary>
/// 取得表單版本歷史查詢處理器
/// </summary>
public class GetFormVersionsQueryHandler : IRequestHandler<GetFormVersionsQuery, List<FormVersionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFormVersionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<FormVersionDto>> Handle(GetFormVersionsQuery request, CancellationToken cancellationToken)
    {
        // 取得表單及專案成員資訊
        var form = await _context.Forms
            .Include(f => f.Project)
                .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("找不到表單");
        }

        // 權限檢查
        var isMember = form.Project.Members.Any(m =>
            m.UserId == request.CurrentUserId && m.RemovedAt == null);

        if (!request.IsSystemAdmin && !isMember)
        {
            throw new UnauthorizedAccessException();
        }

        // 取得版本歷史
        var versions = await _context.FormVersions
            .Include(fv => fv.CreatedBy)
            .Where(fv => fv.FormId == request.FormId)
            .OrderByDescending(fv => fv.CreatedAt)
            .Select(fv => new FormVersionDto
            {
                Id = fv.Id,
                FormId = fv.FormId,
                Version = fv.Version,
                Schema = fv.Schema,
                ChangeNote = fv.ChangeNote,
                CreatedById = fv.CreatedById,
                CreatedByUsername = fv.CreatedBy.Username,
                CreatedAt = fv.CreatedAt,
                IsPublished = fv.IsPublished
            })
            .ToListAsync(cancellationToken);

        return versions;
    }
}
