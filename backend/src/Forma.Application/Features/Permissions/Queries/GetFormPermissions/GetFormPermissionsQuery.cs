using Forma.Application.Features.Permissions.DTOs;
using MediatR;

namespace Forma.Application.Features.Permissions.Queries.GetFormPermissions;

/// <summary>
/// 取得表單權限查詢
/// </summary>
public class GetFormPermissionsQuery : IRequest<FormPermissionSummaryDto>
{
    public Guid FormId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
