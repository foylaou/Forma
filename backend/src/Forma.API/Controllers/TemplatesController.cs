using Forma.Application.Common.Authorization;
using Forma.Application.Common.Interfaces;
using Forma.Application.Common.Models;
using Forma.Application.Features.Templates.Commands.CreateTemplate;
using Forma.Application.Features.Templates.Commands.DeleteTemplate;
using Forma.Application.Features.Templates.Commands.UpdateTemplate;
using Forma.Application.Features.Templates.DTOs;
using Forma.Application.Features.Templates.Queries.GetTemplateById;
using Forma.Application.Features.Templates.Queries.GetTemplates;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Forma.API.Controllers;

/// <summary>
/// 範本管理 API
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Policies.RequireUser)]
public class TemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public TemplatesController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// 取得範本列表
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TemplateListDto>>> GetTemplates(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? category = null,
        [FromQuery] bool? isPublic = null,
        [FromQuery] bool onlyMine = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetTemplatesQuery
        {
            CurrentUserId = _currentUser.UserId!.Value,
            IsSystemAdmin = _currentUser.IsSystemAdmin,
            SearchTerm = searchTerm,
            Category = category,
            IsPublic = isPublic,
            OnlyMine = onlyMine,
            SortBy = sortBy,
            SortDescending = sortDescending,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// 取得範本詳情
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TemplateDto>> GetTemplate(Guid id)
    {
        var query = new GetTemplateByIdQuery
        {
            TemplateId = id,
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
    /// 建立範本
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateTemplate([FromBody] CreateTemplateRequest request)
    {
        var command = new CreateTemplateCommand
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Schema = request.Schema,
            ThumbnailUrl = request.ThumbnailUrl,
            IsPublic = request.IsPublic,
            CurrentUserId = _currentUser.UserId!.Value
        };

        var templateId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTemplate), new { id = templateId }, new { id = templateId });
    }

    /// <summary>
    /// 更新範本
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TemplateDto>> UpdateTemplate(Guid id, [FromBody] UpdateTemplateRequest request)
    {
        var command = new UpdateTemplateCommand
        {
            TemplateId = id,
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Schema = request.Schema,
            ThumbnailUrl = request.ThumbnailUrl,
            IsPublic = request.IsPublic,
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
    /// 刪除範本
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteTemplate(Guid id)
    {
        var command = new DeleteTemplateCommand
        {
            TemplateId = id,
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
}

#region Request DTOs

public class CreateTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string Schema { get; set; } = "{}";
    public string? ThumbnailUrl { get; set; }
    public bool IsPublic { get; set; }
}

public class UpdateTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string Schema { get; set; } = "{}";
    public string? ThumbnailUrl { get; set; }
    public bool? IsPublic { get; set; }
}

#endregion
