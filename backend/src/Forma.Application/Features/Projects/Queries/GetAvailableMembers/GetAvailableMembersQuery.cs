using Forma.Application.Features.Projects.DTOs;
using MediatR;

namespace Forma.Application.Features.Projects.Queries.GetAvailableMembers;

/// <summary>
/// 取得可新增的使用者列表查詢
/// </summary>
public class GetAvailableMembersQuery : IRequest<List<AvailableMemberDto>>
{
    public Guid ProjectId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
    public string? SearchTerm { get; set; }
    public int Limit { get; set; } = 20;
}
