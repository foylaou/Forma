using Forma.Application.Common.Authorization;
using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Users.Commands.ChangePassword;
using Forma.Application.Features.Users.Commands.ToggleUserStatus;
using Forma.Application.Features.Users.Commands.UpdateProfile;
using Forma.Application.Features.Users.Commands.UpdateUser;
using Forma.Application.Features.Users.DTOs;
using Forma.Application.Features.Users.Queries.GetCurrentUser;
using Forma.Application.Features.Users.Queries.GetUserById;
using Forma.Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forma.API.Controllers;

/// <summary>
/// 使用者管理 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 取得當前使用者資料
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileDto>> GetCurrentUser()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized(new { message = "未認證" });
        }

        try
        {
            var result = await _mediator.Send(new GetCurrentUserQuery { UserId = userId.Value });
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 更新當前使用者資料
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized(new { message = "未認證" });
        }

        try
        {
            var command = new UpdateProfileCommand
            {
                UserId = userId.Value,
                Department = request.Department,
                JobTitle = request.JobTitle,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 修改密碼
    /// </summary>
    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized(new { message = "未認證" });
        }

        try
        {
            var command = new ChangePasswordCommand
            {
                UserId = userId.Value,
                CurrentPassword = request.CurrentPassword,
                NewPassword = request.NewPassword,
                ConfirmNewPassword = request.ConfirmNewPassword
            };

            await _mediator.Send(command);
            return Ok(new { message = "密碼已成功變更，請重新登入" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 取得使用者列表 (系統管理員)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = Policies.RequireSystemAdmin)]
    [ProducesResponseType(typeof(PagedResult<UserListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResult<UserListDto>>> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? systemRole = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SystemRole = systemRole,
            IsActive = isActive,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 取得指定使用者 (系統管理員)
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.RequireSystemAdmin)]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetUserById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetUserByIdQuery { UserId = id });
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 更新使用者資料 (系統管理員)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = Policies.RequireSystemAdmin)]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserProfileDto>> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var command = new UpdateUserCommand
            {
                UserId = id,
                Username = request.Username,
                Email = request.Email,
                SystemRole = request.SystemRole,
                Department = request.Department,
                JobTitle = request.JobTitle,
                PhoneNumber = request.PhoneNumber
            };

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 啟用使用者 (系統管理員)
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = Policies.RequireSystemAdmin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        try
        {
            await _mediator.Send(new ToggleUserStatusCommand { UserId = id, Activate = true });
            return Ok(new { message = "使用者已啟用" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 停用使用者 (系統管理員)
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = Policies.RequireSystemAdmin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            await _mediator.Send(new ToggleUserStatusCommand { UserId = id, Activate = false });
            return Ok(new { message = "使用者已停用" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}

#region Request DTOs

/// <summary>
/// 更新個人資料請求
/// </summary>
public class UpdateProfileRequest
{
    /// <summary>
    /// 部門
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// 職稱
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// 電話號碼
    /// </summary>
    public string? PhoneNumber { get; set; }
}

/// <summary>
/// 修改密碼請求
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// 目前密碼
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// 新密碼
    /// </summary>
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// 確認新密碼
    /// </summary>
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

/// <summary>
/// 管理員更新使用者請求
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// 使用者名稱
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// 電子郵件
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 系統角色
    /// </summary>
    public string? SystemRole { get; set; }

    /// <summary>
    /// 部門
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// 職稱
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// 電話號碼
    /// </summary>
    public string? PhoneNumber { get; set; }
}

#endregion
