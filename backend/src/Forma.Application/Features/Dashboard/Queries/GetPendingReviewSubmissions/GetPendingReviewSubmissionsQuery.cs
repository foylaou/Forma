using Forma.Application.Common.Models;
using Forma.Application.Features.Dashboard.DTOs;
using MediatR;

namespace Forma.Application.Features.Dashboard.Queries.GetPendingReviewSubmissions;

public class GetPendingReviewSubmissionsQuery : IRequest<PagedResult<PendingReviewSubmissionDto>>
{
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
