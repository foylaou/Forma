using Forma.Application.Features.Users.DTOs;
using MediatR;

namespace Forma.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// 管理員更新使用者命令
/// </summary>
public class UpdateUserCommand : IRequest<UserProfileDto>
{
    /// <summary>
    /// 目標使用者 ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 系統角色
    /// </summary>
    public string? SystemRole { get; set; }

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
