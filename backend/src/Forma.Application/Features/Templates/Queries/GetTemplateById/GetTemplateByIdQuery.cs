using Forma.Application.Features.Templates.DTOs;
using MediatR;

namespace Forma.Application.Features.Templates.Queries.GetTemplateById;

/// <summary>
/// 取得範本詳情查詢
/// </summary>
public class GetTemplateByIdQuery : IRequest<TemplateDto>
{
    public Guid TemplateId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
