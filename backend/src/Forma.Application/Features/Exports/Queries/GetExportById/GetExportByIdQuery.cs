using Forma.Application.Features.Exports.DTOs;
using MediatR;

namespace Forma.Application.Features.Exports.Queries.GetExportById;

/// <summary>
/// 取得匯出任務詳情查詢
/// </summary>
public class GetExportByIdQuery : IRequest<ExportDto>
{
    public Guid ExportId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
