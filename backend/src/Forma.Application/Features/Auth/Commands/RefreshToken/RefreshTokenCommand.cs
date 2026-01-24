using Forma.Application.Features.Auth.DTOs;
using MediatR;

namespace Forma.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// 刷新 Token 命令
/// </summary>
public class RefreshTokenCommand : IRequest<AuthResponseDto>
{
    /// <summary>
    /// Refresh Token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// 裝置資訊
    /// </summary>
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// IP 位址
    /// </summary>
    public string? IpAddress { get; set; }
}
