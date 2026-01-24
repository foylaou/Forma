using MediatR;

namespace Forma.Application.Features.Notifications.Commands.SendTestEmail;

public class SendTestEmailCommand : IRequest<bool>
{
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
    public string? TargetEmail { get; set; }
}
