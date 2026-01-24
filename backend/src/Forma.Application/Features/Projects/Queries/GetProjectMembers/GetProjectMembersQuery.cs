using Forma.Application.Features.Projects.DTOs;
using MediatR;

namespace Forma.Application.Features.Projects.Queries.GetProjectMembers;

/// <summary>
/// 取得專案成員列表查詢
/// </summary>
public class GetProjectMembersQuery : IRequest<List<ProjectMemberDto>>
{
    /// <summary>
    /// 專案 ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 當前使用者是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }
}
