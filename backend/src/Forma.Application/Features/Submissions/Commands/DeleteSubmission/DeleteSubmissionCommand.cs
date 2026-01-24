using MediatR;

namespace Forma.Application.Features.Submissions.Commands.DeleteSubmission;

/// <summary>
/// 刪除表單提交命令
/// </summary>
public class DeleteSubmissionCommand : IRequest<Unit>
{
    public Guid SubmissionId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
