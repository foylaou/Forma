using Forma.Application.Common.Authorization;
using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Submissions.Commands.CreateSubmission;
using Forma.Application.Features.Submissions.Commands.DeleteSubmission;
using Forma.Application.Features.Submissions.Commands.UpdateSubmission;
using Forma.Application.Features.Submissions.DTOs;
using Forma.Application.Features.Submissions.Queries.GetFormSubmissions;
using Forma.Application.Features.Submissions.Queries.GetSubmissionById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forma.API.Controllers;

/// <summary>
/// 表單提交 API
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Policy = Policies.RequireUser)]
public class SubmissionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public SubmissionsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 提交表單
    /// </summary>
    [HttpPost("submissions")]
    public async Task<ActionResult<Guid>> CreateSubmission([FromBody] CreateSubmissionRequest request)
    {
        var command = new CreateSubmissionCommand
        {
            FormId = request.FormId,
            SubmissionData = request.SubmissionData,
            IsDraft = request.IsDraft,
            CurrentUserId = _currentUser.UserId,
            IpAddress = GetIpAddress()
        };

        try
        {
            var submissionId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetSubmission), new { id = submissionId }, new { id = submissionId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 取得提交詳情
    /// </summary>
    [HttpGet("submissions/{id:guid}")]
    public async Task<ActionResult<SubmissionDto>> GetSubmission(Guid id)
    {
        var query = new GetSubmissionByIdQuery
        {
            SubmissionId = id,
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
    /// 取得表單的所有提交
    /// </summary>
    [HttpGet("forms/{formId:guid}/submissions")]
    public async Task<ActionResult<PagedResult<SubmissionListDto>>> GetFormSubmissions(
        Guid formId,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetFormSubmissionsQuery
        {
            FormId = formId,
            CurrentUserId = _currentUser.UserId!.Value,
            IsSystemAdmin = _currentUser.IsSystemAdmin,
            Status = status,
            SortBy = sortBy,
            SortDescending = sortDescending,
            PageNumber = pageNumber,
            PageSize = pageSize
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
    /// 更新提交
    /// </summary>
    [HttpPut("submissions/{id:guid}")]
    public async Task<ActionResult<SubmissionDto>> UpdateSubmission(Guid id, [FromBody] UpdateSubmissionRequest request)
    {
        var command = new UpdateSubmissionCommand
        {
            SubmissionId = id,
            SubmissionData = request.SubmissionData,
            Status = request.Status,
            CurrentUserId = _currentUser.UserId!.Value,
            IsSystemAdmin = _currentUser.IsSystemAdmin
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
    /// 刪除提交
    /// </summary>
    [HttpDelete("submissions/{id:guid}")]
    public async Task<ActionResult> DeleteSubmission(Guid id)
    {
        var command = new DeleteSubmissionCommand
        {
            SubmissionId = id,
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
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
}

#region Request DTOs

public class CreateSubmissionRequest
{
    public Guid FormId { get; set; }
    public string SubmissionData { get; set; } = "{}";
    public bool IsDraft { get; set; }
}

public class UpdateSubmissionRequest
{
    public string SubmissionData { get; set; } = "{}";
    public string? Status { get; set; }
}

#endregion
