using MediatR;

namespace Forma.Application.Features.Permissions.Commands.RevokePermission;

/// <summary>
/// 移除權限命令
/// </summary>
public class RevokePermissionCommand : IRequest<Unit>
{
    public Guid PermissionId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
