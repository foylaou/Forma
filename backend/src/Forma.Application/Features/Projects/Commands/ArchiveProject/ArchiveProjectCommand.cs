using Forma.Application.Features.Projects.DTOs;
using MediatR;

namespace Forma.Application.Features.Projects.Commands.ArchiveProject;

/// <summary>
/// 封存專案命令
/// </summary>
public class ArchiveProjectCommand : IRequest<ProjectDto>
{
    public Guid ProjectId { get; set; }
    public Guid CurrentUserId { get; set; }
    public bool IsSystemAdmin { get; set; }
}
