using MediatR;

namespace Forma.Application.Features.Organizations.Commands.CreateOrganization;

/// <summary>
/// 建立組織命令
/// </summary>
public class CreateOrganizationCommand : IRequest<Guid>
{
    /// <summary>
    /// 組織名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 組織代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 組織類型
    /// </summary>
    public string Type { get; set; } = "Central";
}
