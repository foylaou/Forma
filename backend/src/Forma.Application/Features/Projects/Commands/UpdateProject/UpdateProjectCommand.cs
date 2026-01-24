using Forma.Application.Features.Projects.DTOs;
using MediatR;

namespace Forma.Application.Features.Projects.Commands.UpdateProject;

/// <summary>
/// 更新專案命令
/// </summary>
public class UpdateProjectCommand : IRequest<ProjectDto>
{
    /// <summary>
    /// 專案 ID
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// 專案名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 專案描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 年度
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 預算
    /// </summary>
    public decimal? Budget { get; set; }

    /// <summary>
    /// 開始日期
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// 專案狀態
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 當前使用者 ID
    /// </summary>
    public Guid CurrentUserId { get; set; }

    /// <summary>
    /// 當前使用者是否為系統管理員
    /// </summary>
    public bool IsSystemAdmin { get; set; }
}
