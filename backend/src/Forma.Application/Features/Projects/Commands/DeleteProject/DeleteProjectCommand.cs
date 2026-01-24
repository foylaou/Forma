using MediatR;

namespace Forma.Application.Features.Projects.Commands.DeleteProject;

/// <summary>
/// 刪除專案命令
/// </summary>
public class DeleteProjectCommand : IRequest<Unit>
{
    /// <summary>
    /// 專案 ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 當前使用者是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }
}
