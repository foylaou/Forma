using Forma.Application.Common.Authorization;
using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Permissions.Commands.GrantPermission;
using Forma.Application.Features.Permissions.Commands.RevokePermission;
using Forma.Application.Features.Permissions.DTOs;
using Forma.Application.Features.Permissions.Queries.GetFormPermissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forma.API.Controllers;

/// <summary>
/// 權限管理 API
/// </summary>
[ApiController]
[Route("api/permissions")]
[Authorize(Policy = Policies.RequireUser)]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public PermissionsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 取得表單權限
    /// </summary>
    [HttpGet("form/{formId:guid}")]
    public async Task<ActionResult<FormPermissionSummaryDto>> GetFormPermissions(Guid formId)
    {
        var query = new GetFormPermissionsQuery
        {
            FormId = formId,
            CurrentUserId = _currentUser.UserId!.Value,
            IsSystemAdmin = _currentUser.IsSystemAdmin
        };

        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// 授予權限
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> GrantPermission([FromBody] GrantPermissionRequest request)
    {
        var command = new GrantPermissionCommand
        {
            FormId = request.FormId,
            UserId = request.UserId,
            ProjectMemberRole = request.ProjectMemberRole,
            PermissionType = request.PermissionType,
            CurrentUserId = _currentUser.UserId!.Value,
            IsSystemAdmin = _currentUser.IsSystemAdmin
        };

        try
        {
            var permissionId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetFormPermissions), new { formId = request.FormId }, new { id = permissionId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 移除權限
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> RevokePermission(Guid id)
    {
        var command = new RevokePermissionCommand
        {
            PermissionId = id,
            CurrentUserId = _currentUser.UserId!.Value,
            IsSystemAdmin = _currentUser.IsSystemAdmin
        };

        try
        {
            await _mediator.Send(command);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}

#region Request DTOs

public class GrantPermissionRequest
{
    public Guid FormId { get; set; }
    public Guid? UserId { get; set; }
    public string? ProjectMemberRole { get; set; }
    public string PermissionType { get; set; } = string.Empty;
}

#endregion
