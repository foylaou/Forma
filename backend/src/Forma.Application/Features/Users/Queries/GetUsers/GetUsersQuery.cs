using Forma.Application.Common.Models;
using Forma.Application.Features.Users.DTOs;
using MediatR;

namespace Forma.Application.Features.Users.Queries.GetUsers;

/// <summary>
/// 取得使用者列表查詢
/// </summary>
public class GetUsersQuery : IRequest<PagedResult<UserListDto>>
{
    /// <summary>
    /// 頁碼 (從 1 開始)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 每頁筆數
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// 搜尋關鍵字 (使用者名稱、電子郵件)
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// 篩選系統角色
    /// </summary>
    public string? SystemRole { get; set; }

    /// <summary>
    /// 篩選啟用狀態
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 排序欄位
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 是否降序排列
    /// </summary>
    public bool SortDescending { get; set; }
}
