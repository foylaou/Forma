using MediatR;

namespace Forma.Application.Features.Submissions.Commands.CreateSubmission;

/// <summary>
/// 建立表單提交命令
/// </summary>
public class CreateSubmissionCommand : IRequest<Guid>
{
    public Guid FormId { get; set; }
    public string SubmissionData { get; set; } = "{}";
    public bool IsDraft { get; set; }
    public Guid? CurrentUserId { get; set; }
    public string? IpAddress { get; set; }
}
