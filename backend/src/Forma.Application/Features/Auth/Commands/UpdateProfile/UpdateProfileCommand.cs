using Forma.Application.Features.Auth.DTOs;
using MediatR;

namespace Forma.Application.Features.Auth.Commands.UpdateProfile;

/// <summary>
/// 更新個人資料命令
/// </summary>
public class UpdateProfileCommand : IRequest<ProfileDto>
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 部門
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// 職稱
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// 電話號碼
    /// </summary>
    public string? PhoneNumber { get; set; }
}
