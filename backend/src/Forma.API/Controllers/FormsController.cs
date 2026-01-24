using Forma.Application.Common.Authorization;
using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Forms.Commands.CloneForm;
using Forma.Application.Features.Forms.Commands.CreateForm;
using Forma.Application.Features.Forms.Commands.DeleteForm;
using Forma.Application.Features.Forms.Commands.PublishForm;
using Forma.Application.Features.Forms.Commands.UnpublishForm;
using Forma.Application.Features.Forms.Commands.UpdateForm;
using Forma.Application.Features.Forms.DTOs;
using Forma.Application.Features.Forms.Queries.GetFormById;
using Forma.Application.Features.Forms.Queries.GetForms;
using Forma.Application.Features.Forms.Queries.GetFormVersions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forma.API.Controllers;

/// <summary>
/// 表單管理 API
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Policy = Policies.RequireUser)]
public class FormsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public FormsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 取得專案的表單列表
    /// </summary>
    [HttpGet("projects/{projectId:guid}/forms")]
    public async Task<ActionResult<PagedResult<FormListDto>>> GetForms(
        Guid projectId,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isPublished = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetFormsQuery
        {
            ProjectId = projectId,
            CurrentUserId = _currentUser.UserId!.Value,
            IsSystemAdmin = _currentUser.IsSystemAdmin,
            SearchTerm = searchTerm,
            IsPublished = isPublished,
            IsActive = isActive,
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
    /// 取得表單詳情
    /// </summary>
    [HttpGet("forms/{id:guid}")]
    public async Task<ActionResult<FormDto>> GetForm(Guid id)
    {
        var query = new GetFormByIdQuery
        {
            FormId = id,
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
    /// 建立表單
    /// </summary>
    [HttpPost("projects/{projectId:guid}/forms")]
    public async Task<ActionResult<Guid>> CreateForm(Guid projectId, [FromBody] CreateFormRequest request)
    {
        var command = new CreateFormCommand
        {
            ProjectId = projectId,
            Name = request.Name,
            Description = request.Description,
            Schema = request.Schema,
            TemplateId = request.TemplateId,
            AccessControl = request.AccessControl ?? "Private",
            CurrentUserId = _currentUser.UserId!.Value,
            IsSystemAdmin = _currentUser.IsSystemAdmin
        };

        try
        {
            var formId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetForm), new { id = formId }, new { id = formId });
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
    /// 更新表單
    /// </summary>
    [HttpPut("forms/{id:guid}")]
    public async Task<ActionResult<FormDto>> UpdateForm(Guid id, [FromBody] UpdateFormRequest request)
    {
        var command = new UpdateFormCommand
        {
            FormId = id,
            Name = request.Name,
            Description = request.Description,
            Schema = request.Schema,
            AccessControl = request.AccessControl,
            IsActive = request.IsActive,
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
    }

    /// <summary>
    /// 刪除表單
    /// </summary>
    [HttpDelete("forms/{id:guid}")]
    public async Task<ActionResult> DeleteForm(Guid id)
    {
        var command = new DeleteFormCommand
        {
            FormId = id,
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

    /// <summary>
    /// 發布表單
    /// </summary>
    [HttpPost("forms/{id:guid}/publish")]
    public async Task<ActionResult<FormDto>> PublishForm(Guid id)
    {
        var command = new PublishFormCommand
        {
            FormId = id,
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
    /// 下架表單
    /// </summary>
    [HttpPost("forms/{id:guid}/unpublish")]
    public async Task<ActionResult<FormDto>> UnpublishForm(Guid id)
    {
        var command = new UnpublishFormCommand
        {
            FormId = id,
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
    /// 複製表單
    /// </summary>
    [HttpPost("forms/{id:guid}/clone")]
    public async Task<ActionResult<Guid>> CloneForm(Guid id, [FromBody] CloneFormRequest? request = null)
    {
        var command = new CloneFormCommand
        {
            FormId = id,
            TargetProjectId = request?.TargetProjectId,
            NewName = request?.NewName,
            CurrentUserId = _currentUser.UserId!.Value,
            IsSystemAdmin = _currentUser.IsSystemAdmin
        };

        try
        {
            var newFormId = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetForm), new { id = newFormId }, new { id = newFormId });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>
    /// 取得表單版本歷史
    /// </summary>
    [HttpGet("forms/{id:guid}/versions")]
    public async Task<ActionResult<List<FormVersionDto>>> GetFormVersions(Guid id)
    {
        var query = new GetFormVersionsQuery
        {
            FormId = id,
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
}

#region Request DTOs

public class CreateFormRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Schema { get; set; } = "{}";
    public Guid? TemplateId { get; set; }
    public string? AccessControl { get; set; }
}

public class UpdateFormRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Schema { get; set; } = "{}";
    public string? AccessControl { get; set; }
    public bool? IsActive { get; set; }
}

public class CloneFormRequest
{
    public Guid? TargetProjectId { get; set; }
    public string? NewName { get; set; }
}

#endregion
