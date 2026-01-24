using Forma.Application.Common.Models;
using Forma.Application.Features.Forms.DTOs;
using MediatR;

namespace Forma.Application.Features.Forms.Queries.GetForms;

/// <summary>
/// 取得專案表單列表查詢
/// </summary>
public class GetFormsQuery : IRequest<PagedResult<FormListDto>>
{
    /// <summary>
    /// 專案 ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }

    /// <summary>
    /// 搜尋關鍵字
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// 是否只顯示已發布
    /// </summary>
    public bool? IsPublished { get; set; }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 排序欄位
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 是否降序
    /// </summary>
    public bool SortDescending { get; set; }

    /// <summary>
    /// 頁碼
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 每頁筆數
    /// </summary>
    public int PageSize { get; set; } = 20;
}
