using Forma.Application.Features.Dashboard.DTOs;
using MediatR;

namespace Forma.Application.Features.Dashboard.Queries.GetPersonalStatistics;

public class GetPersonalStatisticsQuery : IRequest<PersonalStatisticsDto>
{
    public Guid CurrentUserId { get; set; }
}
