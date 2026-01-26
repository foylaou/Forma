using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Exports.DTOs;
using Forma.Domain.Entities;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Exports.Commands.CreateExport;

/// <summary>
/// 建立匯出任務命令處理器
/// </summary>
public class CreateExportCommandHandler : IRequestHandler<CreateExportCommand, ExportDto>
{
    private readonly IApplicationDbContext _context;

    public CreateExportCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ExportDto> Handle(CreateExportCommand request, CancellationToken cancellationToken)
    {
        // 取得表單
        var form = await _context.Forms
            .Include(f => f.Project)
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (form == null)
        {
            throw new KeyNotFoundException("找不到表單");
        }

        // 權限檢查
        if (!request.IsSystemAdmin)
        {
            var membership = await _context.ProjectMembers
                .FirstOrDefaultAsync(pm =>
                    pm.ProjectId == form.ProjectId &&
                    pm.UserId == request.CurrentUserId &&
                    pm.RemovedAt == null,
                    cancellationToken);

            if (membership == null || membership.Role < ProjectRole.Analyst)
            {
                throw new UnauthorizedAccessException();
            }
        }

        // 取得使用者資訊
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.CurrentUserId, cancellationToken);

        // 建立匯出任務
        var export = new Export
        {
            Id = Guid.CreateVersion7(),
            FormId = request.FormId,
            Format = request.Format.ToUpperInvariant(),
            Filters = request.Filters,
            Status = "Pending",
            CreatedById = request.CurrentUserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(24) // 匯出檔案 24 小時後過期
        };

        _context.Exports.Add(export);
        await _context.SaveChangesAsync(cancellationToken);

        return new ExportDto
        {
            Id = export.Id,
            FormId = export.FormId,
            FormName = form.Name,
            Format = export.Format,
            Filters = export.Filters,
            Status = export.Status,
            FileName = export.FileName,
            FileSize = export.FileSize,
            RecordCount = export.RecordCount,
            ErrorMessage = export.ErrorMessage,
            CreatedById = export.CreatedById,
            CreatedByUsername = user?.Username ?? string.Empty,
            CreatedAt = export.CreatedAt,
            CompletedAt = export.CompletedAt,
            ExpiresAt = export.ExpiresAt
        };
    }
}
