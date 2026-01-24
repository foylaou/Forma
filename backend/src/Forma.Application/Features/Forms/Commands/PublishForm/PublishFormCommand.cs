using Forma.Application.Features.Forms.DTOs;
using MediatR;

namespace Forma.Application.Features.Forms.Commands.PublishForm;

/// <summary>
/// 發布表單命令
/// </summary>
public class PublishFormCommand : IRequest<FormDto>
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
