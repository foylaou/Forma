using MediatR;

namespace Forma.Application.Features.Forms.Commands.DeleteForm;

/// <summary>
/// 刪除表單命令
/// </summary>
public class DeleteFormCommand : IRequest<Unit>
{
    /// <summary>
    /// 表單 ID
    /// </summary>
    public Guid FormId { get; set; }

    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }
}
