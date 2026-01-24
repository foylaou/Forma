using MediatR;

namespace Forma.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// 建立專案命令
/// </summary>
public class CreateProjectCommand : IRequest<Guid>
{
    /// <summary>
    /// 組織 ID
    /// </summary>
    public Guid OrganizationId { get; set; }

    /// <summary>
    /// 專案名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 專案代碼
    /// </summary>
    public string Code { get; set; } = string.Empty;

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
    /// 當前使用者 ID (建立者)
    /// </summary>
    public Guid CurrentUserId { get; set; }
}
