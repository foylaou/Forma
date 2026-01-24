using Forma.Application.Features.Dashboard.DTOs;
using MediatR;

namespace Forma.Application.Features.Dashboard.Queries.GetDashboardSummary;

/// <summary>
/// 取得儀表板摘要查詢
/// </summary>
public class GetDashboardSummaryQuery : IRequest<DashboardSummaryDto>
{
    public Guid CurrentUserId { get; set; }
}
