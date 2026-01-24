using MediatR;

namespace Forma.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// 修改密碼命令
/// </summary>
public class ChangePasswordCommand : IRequest<bool>
{
    /// <summary>
    /// 當前使用者 ID (由系統設定)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 目前密碼
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密碼
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 確認新密碼
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
