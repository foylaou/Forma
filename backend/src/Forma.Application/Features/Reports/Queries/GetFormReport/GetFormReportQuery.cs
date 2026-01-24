using Forma.Application.Features.Reports.DTOs;
using MediatR;

namespace Forma.Application.Features.Reports.Queries.GetFormReport;

/// <summary>
/// 取得表單報告查詢
/// </summary>
public class GetFormReportQuery : IRequest<ReportDto>
{
    public Guid FormId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
