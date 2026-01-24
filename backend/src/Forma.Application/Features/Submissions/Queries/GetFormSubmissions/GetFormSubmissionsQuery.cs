using Forma.Application.Common.Models;
using Forma.Application.Features.Submissions.DTOs;
using MediatR;

namespace Forma.Application.Features.Submissions.Queries.GetFormSubmissions;

/// <summary>
/// 取得表單提交列表查詢
/// </summary>
public class GetFormSubmissionsQuery : IRequest<PagedResult<SubmissionListDto>>
{
    public Guid FormId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
    public string? Status { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
