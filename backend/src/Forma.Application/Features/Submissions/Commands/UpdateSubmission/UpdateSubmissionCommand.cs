using Forma.Application.Features.Submissions.DTOs;
using MediatR;

namespace Forma.Application.Features.Submissions.Commands.UpdateSubmission;

/// <summary>
/// 更新表單提交命令
/// </summary>
public class UpdateSubmissionCommand : IRequest<SubmissionDto>
{
    public Guid SubmissionId { get; set; }
    public string SubmissionData { get; set; } = "{}";
    public string? Status { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
