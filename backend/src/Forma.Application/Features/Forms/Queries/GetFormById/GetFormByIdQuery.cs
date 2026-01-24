using Forma.Application.Features.Forms.DTOs;
using MediatR;

namespace Forma.Application.Features.Forms.Queries.GetFormById;

/// <summary>
/// 取得表單詳情查詢
/// </summary>
public class GetFormByIdQuery : IRequest<FormDto>
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
