using Forma.Application.Features.Exports.DTOs;
using MediatR;

namespace Forma.Application.Features.Exports.Commands.CreateExport;

/// <summary>
/// 建立匯出任務命令
/// </summary>
public class CreateExportCommand : IRequest<ExportDto>
{
    public Guid FormId { get; set; }
    public string Format { get; set; } = "CSV";
    public string? Filters { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
