using Forma.Application.Common.Authorization;
using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Dashboard.DTOs;
using Forma.Application.Features.Dashboard.Queries.GetDashboardSummary;
using Forma.Application.Features.Dashboard.Queries.GetPendingReviewSubmissions;
using Forma.Application.Features.Dashboard.Queries.GetPendingTasks;
using Forma.Application.Features.Dashboard.Queries.GetPersonalStatistics;
using Forma.Application.Features.Dashboard.Queries.GetProjectsStats;
using Forma.Application.Features.Dashboard.Queries.GetRecentActivities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forma.API.Controllers;

/// <summary>
/// 儀表板 API
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize(Policy = Policies.RequireUser)]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 取得個人儀表板摘要
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var query = new GetDashboardSummaryQuery
        {
            CurrentUserId = _currentUser.UserId!.Value
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 取得待辦事項（跨計畫）
    /// </summary>
    [HttpGet("pending-tasks")]
    public async Task<ActionResult<List<PendingTaskDto>>> GetPendingTasks([FromQuery] int limit = 20)
    {
        var query = new GetPendingTasksQuery
        {
            CurrentUserId = _currentUser.UserId!.Value,
            Limit = limit
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 取得最近活動
    /// </summary>
    [HttpGet("recent-activities")]
    public async Task<ActionResult<List<RecentActivityDto>>> GetRecentActivities([FromQuery] int limit = 20)
    {
        var query = new GetRecentActivitiesQuery
        {
            CurrentUserId = _currentUser.UserId!.Value,
            Limit = limit
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 取得個人統計資料
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<PersonalStatisticsDto>> GetStatistics()
    {
        var query = new GetPersonalStatisticsQuery
        {
            CurrentUserId = _currentUser.UserId!.Value
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 取得所有參與計畫的統計
    /// </summary>
    [HttpGet("projects/stats")]
    public async Task<ActionResult<List<ProjectStatsDto>>> GetProjectsStats()
    {
        var query = new GetProjectsStatsQuery
        {
            CurrentUserId = _currentUser.UserId!.Value
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

/// <summary>
/// 提交擴展 API（跨計畫查詢）
/// </summary>
[ApiController]
[Route("api/submissions")]
[Authorize(Policy = Policies.RequireUser)]
public class SubmissionsExtController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public SubmissionsExtController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 取得所有待審核提交（跨計畫）
    /// </summary>
    [HttpGet("pending-review")]
    public async Task<ActionResult<PagedResult<PendingReviewSubmissionDto>>> GetPendingReview(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetPendingReviewSubmissionsQuery
        {
            CurrentUserId = _currentUser.UserId!.Value,
            IsSystemAdmin = _currentUser.IsSystemAdmin,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
