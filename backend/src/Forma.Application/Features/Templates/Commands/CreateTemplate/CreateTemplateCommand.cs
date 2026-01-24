using MediatR;

namespace Forma.Application.Features.Templates.Commands.CreateTemplate;

/// <summary>
/// 建立範本命令
/// </summary>
public class CreateTemplateCommand : IRequest<Guid>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string Schema { get; set; } = "{}";
    public string? ThumbnailUrl { get; set; }
    public bool IsPublic { get; set; }
    public Guid CurrentUserId { get; set; }
}
