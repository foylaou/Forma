using Forma.Application.Features.Forms.DTOs;
using MediatR;

namespace Forma.Application.Features.Forms.Commands.UnpublishForm;

/// <summary>
/// 下架表單命令
/// </summary>
public class UnpublishFormCommand : IRequest<FormDto>
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
