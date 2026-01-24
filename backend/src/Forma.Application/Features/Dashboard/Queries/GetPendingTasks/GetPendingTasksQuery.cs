using Forma.Application.Features.Dashboard.DTOs;
using MediatR;

namespace Forma.Application.Features.Dashboard.Queries.GetPendingTasks;

public class GetPendingTasksQuery : IRequest<List<PendingTaskDto>>
{
    public Guid CurrentUserId { get; set; }
    public int Limit { get; set; } = 20;
}
