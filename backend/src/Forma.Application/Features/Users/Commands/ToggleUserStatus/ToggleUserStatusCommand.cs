using MediatR;

namespace Forma.Application.Features.Users.Commands.ToggleUserStatus;

/// <summary>
/// 切換使用者狀態命令
/// </summary>
public class ToggleUserStatusCommand : IRequest<bool>
{
    /// <summary>
    /// 目標使用者 ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool Activate { get; set; }
}
