using Forma.Application.Common.Models;
using Forma.Application.Features.Projects.DTOs;
using MediatR;

namespace Forma.Application.Features.Projects.Queries.GetProjects;

/// <summary>
/// 取得專案列表查詢
/// </summary>
public class GetProjectsQuery : IRequest<PagedResult<ProjectListDto>>
{
    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 當前使用者是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }

    /// <summary>
    /// 頁碼
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 每頁筆數
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 搜尋關鍵字
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// 篩選年度
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// 篩選狀態
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 僅顯示我參與的專案
    /// </summary>
    public bool OnlyMyProjects { get; set; } = true;

    /// <summary>
    /// 排序欄位
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 是否降序
    /// </summary>
    public bool SortDescending { get; set; }
}
