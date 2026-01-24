using MediatR;

namespace Forma.Application.Features.Forms.Commands.CloneForm;

/// <summary>
/// 複製表單命令
/// </summary>
public class CloneFormCommand : IRequest<Guid>
{
    /// <summary>
    /// 來源表單 ID
    /// </summary>
    public Guid FormId { get; set; }

    /// <summary>
    /// 目標專案 ID (可選，預設為同專案)
    /// </summary>
    public Guid? TargetProjectId { get; set; }

    /// <summary>
    /// 新表單名稱 (可選)
    /// </summary>
    public string? NewName { get; set; }

    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }
}
