using Forma.Application.Features.Dashboard.DTOs;
using MediatR;

namespace Forma.Application.Features.Dashboard.Queries.GetRecentActivities;

public class GetRecentActivitiesQuery : IRequest<List<RecentActivityDto>>
{
    public Guid CurrentUserId { get; set; }
    public int Limit { get; set; } = 20;
}
