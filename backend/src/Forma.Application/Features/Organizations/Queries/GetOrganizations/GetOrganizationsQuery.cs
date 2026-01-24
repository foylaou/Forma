using Forma.Application.Common.Models;
using Forma.Application.Features.Organizations.DTOs;
using MediatR;

namespace Forma.Application.Features.Organizations.Queries.GetOrganizations;

/// <summary>
/// 取得組織列表查詢
/// </summary>
public class GetOrganizationsQuery : IRequest<PagedResult<OrganizationListDto>>
{
    /// <summary>
    /// 搜尋關鍵字
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// 組織類型篩選
    /// </summary>
    public string? Type { get; set; }

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
