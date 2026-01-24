using Forma.Application.Features.Dashboard.DTOs;
using MediatR;

namespace Forma.Application.Features.Dashboard.Queries.GetProjectsStats;

public class GetProjectsStatsQuery : IRequest<List<ProjectStatsDto>>
{
    public Guid CurrentUserId { get; set; }
}
