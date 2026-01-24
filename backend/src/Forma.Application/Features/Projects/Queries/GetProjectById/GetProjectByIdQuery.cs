using Forma.Application.Features.Projects.DTOs;
using MediatR;

namespace Forma.Application.Features.Projects.Queries.GetProjectById;

/// <summary>
/// 取得專案詳情查詢
/// </summary>
public class GetProjectByIdQuery : IRequest<ProjectDto>
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
