using Forma.Application.Features.Organizations.DTOs;
using MediatR;

namespace Forma.Application.Features.Organizations.Commands.UpdateOrganization;

/// <summary>
/// 更新組織命令
/// </summary>
public class UpdateOrganizationCommand : IRequest<OrganizationDto>
{
    /// <summary>
    /// 組織 ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// 組織名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 組織類型
    /// </summary>
    public string? Type { get; set; }
}
