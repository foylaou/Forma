using MediatR;

namespace Forma.Application.Features.Projects.Commands.RemoveProjectMember;

/// <summary>
/// 移除專案成員命令
/// </summary>
public class RemoveProjectMemberCommand : IRequest<Unit>
{
    /// <summary>
    /// 專案 ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// 要移除的成員使用者 ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 當前使用者是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }
}
