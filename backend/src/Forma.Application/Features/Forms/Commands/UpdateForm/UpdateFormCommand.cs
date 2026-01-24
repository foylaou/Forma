using Forma.Application.Features.Forms.DTOs;
using MediatR;

namespace Forma.Application.Features.Forms.Commands.UpdateForm;

/// <summary>
/// 更新表單命令
/// </summary>
public class UpdateFormCommand : IRequest<FormDto>
{
    /// <summary>
    /// 表單 ID
    /// </summary>
    public Guid FormId { get; set; }

    /// <summary>
    /// 表單名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 表單結構定義 (JSON)
    /// </summary>
    public string Schema { get; set; } = "{}";

    /// <summary>
    /// 存取控制
    /// </summary>
    public string? AccessControl { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }
}
