using Forma.Application.Common.Models;
using Forma.Application.Features.Templates.DTOs;
using MediatR;

namespace Forma.Application.Features.Templates.Queries.GetTemplates;

/// <summary>
/// 取得範本列表查詢
/// </summary>
public class GetTemplatesQuery : IRequest<PagedResult<TemplateListDto>>
{
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public bool? IsPublic { get; set; }
    public bool OnlyMine { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
