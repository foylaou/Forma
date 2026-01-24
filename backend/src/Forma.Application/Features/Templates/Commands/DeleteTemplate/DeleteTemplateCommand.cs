using MediatR;

namespace Forma.Application.Features.Templates.Commands.DeleteTemplate;

/// <summary>
/// 刪除範本命令
/// </summary>
public class DeleteTemplateCommand : IRequest<Unit>
{
    public Guid TemplateId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
