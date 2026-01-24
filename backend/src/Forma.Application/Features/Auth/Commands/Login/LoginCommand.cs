using Forma.Application.Features.Auth.DTOs;
using MediatR;

namespace Forma.Application.Features.Auth.Commands.Login;

/// <summary>
/// 登入命令
/// </summary>
public class LoginCommand : IRequest<AuthResponseDto>
{
    /// <summary>
    /// 電子郵件或使用者名稱
    /// </summary>
    public string EmailOrUsername { get; set; } = string.Empty;

    /// <summary>
    /// 密碼
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 裝置資訊
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// IP 位址
    /// </summary>
    public string? IpAddress { get; set; }
}
