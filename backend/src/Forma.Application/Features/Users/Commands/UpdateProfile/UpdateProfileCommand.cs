using Forma.Application.Features.Users.DTOs;
using MediatR;

namespace Forma.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// 更新個人資料命令
/// </summary>
public class UpdateProfileCommand : IRequest<UserProfileDto>
{
    /// <summary>
    /// 當前使用者 ID (由系統設定)
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
