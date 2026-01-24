using MediatR;

namespace Forma.Application.Features.Organizations.Commands.DeleteOrganization;

/// <summary>
/// 刪除組織命令
/// </summary>
public class DeleteOrganizationCommand : IRequest<Unit>
{
    /// <summary>
    /// 組織 ID
    /// </summary>
    public Guid OrganizationId { get; set; }
}
