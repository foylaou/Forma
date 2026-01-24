using Forma.Application.Common.Models;
using Forma.Application.Features.Projects.DTOs;
using MediatR;

namespace Forma.Application.Features.Projects.Queries.GetParticipatedProjects;

/// <summary>
/// 取得我參與的專案查詢
/// </summary>
public class GetParticipatedProjectsQuery : IRequest<PagedResult<ProjectListDto>>
{
    public Guid CurrentUserId { get; set; }
    public string? SearchTerm { get; set; }
    public int? Year { get; set; }
    public string? Status { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
