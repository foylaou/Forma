using Forma.Application.Features.Submissions.DTOs;
using MediatR;

namespace Forma.Application.Features.Submissions.Queries.GetSubmissionById;

/// <summary>
/// 取得提交詳情查詢
/// </summary>
public class GetSubmissionByIdQuery : IRequest<SubmissionDto>
{
    public Guid SubmissionId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
