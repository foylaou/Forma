using Forma.Application.Features.Projects.DTOs;
using MediatR;

namespace Forma.Application.Features.Projects.Commands.UpdateProjectMember;

/// <summary>
/// 更新專案成員命令
/// </summary>
public class UpdateProjectMemberCommand : IRequest<ProjectMemberDto>
{
    /// <summary>
    /// 專案 ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// 成員的使用者 ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 新的專案角色
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 當前使用者是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }
}
