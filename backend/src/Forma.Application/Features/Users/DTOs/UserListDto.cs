namespace Forma.Application.Features.Users.DTOs;

/// <summary>
/// 使用者列表 DTO
/// </summary>
public class UserListDto
{
    /// <summary>
    /// 使用者 ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 系統角色
    /// </summary>
    public string SystemRole { get; set; } = string.Empty;

    /// <summary>
    /// 部門
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 最後登入時間
    /// </summary>
    public DateTime? LastLoginAt { get; set; }
}
