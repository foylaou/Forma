using MediatR;

namespace Forma.Application.Features.Permissions.Commands.GrantPermission;

/// <summary>
/// 授予權限命令
/// </summary>
public class GrantPermissionCommand : IRequest<Guid>
{
    public Guid FormId { get; set; }
    public Guid? UserId { get; set; }
    public string? ProjectMemberRole { get; set; }
    public string PermissionType { get; set; } = string.Empty;
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
