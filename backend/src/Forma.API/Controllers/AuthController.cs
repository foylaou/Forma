using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Auth.Commands.Login;
using Forma.Application.Features.Auth.Commands.Logout;
using Forma.Application.Features.Auth.Commands.RefreshToken;
using Forma.Application.Features.Auth.Commands.Register;
using Forma.Application.Features.Auth.Commands.UpdateProfile;
using Forma.Application.Features.Auth.DTOs;
using Forma.Application.Features.Auth.Queries.GetProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forma.API.Controllers;

/// <summary>
/// 認證 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// 使用者登入
    /// </summary>
    /// <param name="command">登入資訊</param>
    /// <returns>認證回應</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
    {
        command.IpAddress = GetIpAddress();
        command.DeviceInfo = GetDeviceInfo();

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 使用者註冊
    /// </summary>
    /// <param name="command">註冊資訊</param>
    /// <returns>認證回應</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterCommand command)
    {
        command.IpAddress = GetIpAddress();
        command.DeviceInfo = GetDeviceInfo();

        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(Login), result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 刷新 Token
    /// </summary>
    /// <param name="command">刷新 Token 資訊</param>
    /// <returns>認證回應</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        command.IpAddress = GetIpAddress();
        command.DeviceInfo = GetDeviceInfo();

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 登出 (單一裝置)
    /// </summary>
    /// <param name="refreshToken">Refresh Token (可選)</param>
    /// <returns>登出結果</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized(new { message = "未認證" });
        }

        var command = new LogoutCommand
        {
            UserId = userId.Value,
            RefreshToken = request?.RefreshToken,
            LogoutAll = false
        };

        await _mediator.Send(command);
        return Ok(new { message = "已成功登出" });
    }

    /// <summary>
    /// 登出所有裝置
    /// </summary>
    /// <returns>登出結果</returns>
    [HttpPost("logout-all")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized(new { message = "未認證" });
        }

        var command = new LogoutCommand
        {
            UserId = userId.Value,
            LogoutAll = true
        };

        await _mediator.Send(command);
        return Ok(new { message = "已成功登出所有裝置" });
    }

    /// <summary>
    /// 取得個人資料
    /// </summary>
    /// <returns>個人資料</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized(new { message = "未認證" });
        }

        try
        {
            var result = await _mediator.Send(new GetProfileQuery { UserId = userId.Value });
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 更新個人資料
    /// </summary>
    /// <param name="request">更新資料</param>
    /// <returns>更新後的個人資料</returns>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = _currentUserService.UserId;
        if (userId == null)
        {
            return Unauthorized(new { message = "未認證" });
        }

        var command = new UpdateProfileCommand
        {
            UserId = userId.Value,
            Department = request.Department,
            JobTitle = request.JobTitle,
            PhoneNumber = request.PhoneNumber
        };

        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    private string? GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
        {
            return Request.Headers["X-Forwarded-For"].FirstOrDefault();
        }

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
    }

    private string? GetDeviceInfo()
    {
        return Request.Headers.UserAgent.FirstOrDefault();
    }
}

/// <summary>
/// 登出請求
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// Refresh Token (可選)
    /// </summary>
    public string? RefreshToken { get; set; }
}
