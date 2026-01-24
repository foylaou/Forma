using MediatR;

namespace Forma.Application.Features.Auth.Commands.Logout;

/// <summary>
/// 登出命令
/// </summary>
public class LogoutCommand : IRequest<bool>
{
    /// <summary>
    /// Refresh Token
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>
    /// 是否登出所有裝置
    /// </summary>
    public bool LogoutAll { get; set; }

    /// <summary>
    /// 當前使用者 ID (由系統設定)
    /// </summary>
    public Guid UserId { get; set; }
}
