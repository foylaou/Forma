using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Exports.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Exports.Queries.GetExportById;

/// <summary>
/// 取得匯出任務詳情查詢處理器
/// </summary>
public class GetExportByIdQueryHandler : IRequestHandler<GetExportByIdQuery, ExportDto>
{
    private readonly IApplicationDbContext _context;

    public GetExportByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExportDto> Handle(GetExportByIdQuery request, CancellationToken cancellationToken)
    {
        var export = await _context.Exports
            .Include(e => e.Form)
            .Include(e => e.CreatedBy)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.ExportId, cancellationToken);

        if (export == null)
        {
            throw new KeyNotFoundException("找不到匯出任務");
        }

        // 權限檢查
        if (!request.IsSystemAdmin && export.CreatedById != request.CurrentUserId)
        {
            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm =>
                    pm.ProjectId == export.Form.ProjectId &&
                    pm.UserId == request.CurrentUserId &&
                    pm.RemovedAt == null,
                    cancellationToken);

            if (membership == null || membership.Role < ProjectRole.Analyst)
            {
                throw new UnauthorizedAccessException();
            }
        }

        return new ExportDto
        {
            Id = export.Id,
            FormId = export.FormId,
            FormName = export.Form.Name,
            Format = export.Format,
            Filters = export.Filters,
            Status = export.Status,
            FileName = export.FileName,
            FileSize = export.FileSize,
            RecordCount = export.RecordCount,
            ErrorMessage = export.ErrorMessage,
            CreatedById = export.CreatedById,
            CreatedByUsername = export.CreatedBy.Username,
            CreatedAt = export.CreatedAt,
            CompletedAt = export.CompletedAt,
            ExpiresAt = export.ExpiresAt,
            DownloadUrl = export.Status == "Completed" ? $"/api/exports/{export.Id}/download" : null
        };
    }
}
