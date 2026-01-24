using Forma.Application.Features.Organizations.DTOs;
using MediatR;

namespace Forma.Application.Features.Organizations.Queries.GetOrganizationById;

/// <summary>
/// 取得組織詳情查詢
/// </summary>
public class GetOrganizationByIdQuery : IRequest<OrganizationDto>
{
    /// <summary>
    /// 組織 ID
    /// </summary>
    public Guid OrganizationId { get; set; }
}
