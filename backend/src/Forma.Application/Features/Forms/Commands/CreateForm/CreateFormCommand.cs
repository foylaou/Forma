using MediatR;

namespace Forma.Application.Features.Forms.Commands.CreateForm;

/// <summary>
/// 建立表單命令
/// </summary>
public class CreateFormCommand : IRequest<Guid>
{
    /// <summary>
    /// 專案 ID
    /// </summary>
    public Guid ProjectId { get; set; }

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
    /// 範本 ID (選填)
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// 存取控制
    /// </summary>
    public string AccessControl { get; set; } = "Private";

    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }
}
