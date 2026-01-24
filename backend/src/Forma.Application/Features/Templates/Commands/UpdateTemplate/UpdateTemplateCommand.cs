using Forma.Application.Features.Templates.DTOs;
using MediatR;

namespace Forma.Application.Features.Templates.Commands.UpdateTemplate;

/// <summary>
/// 更新範本命令
/// </summary>
public class UpdateTemplateCommand : IRequest<TemplateDto>
{
    public Guid TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string Schema { get; set; } = "{}";
    public string? ThumbnailUrl { get; set; }
    public bool? IsPublic { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
