using Forma.Application.Common.Interfaces;
using Forma.Domain.Entities;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// 建立專案命令處理器
/// </summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProjectCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        // 檢查專案代碼是否已存在
        var codeExists = await _context.Projects
            .AnyAsync(p => p.Code == request.Code, cancellationToken);

        if (codeExists)
        {
            throw new InvalidOperationException("專案代碼已存在");
        }

        // 建立專案
        var project = new Project
        {
            Id = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Year = request.Year,
            Budget = request.Budget,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = ProjectStatus.Draft,
            CreatedById = request.CurrentUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);

        // 將建立者加入為專案擁有者
        var projectMember = new ProjectMember
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            UserId = request.CurrentUserId,
            Role = ProjectRole.Owner,
            AddedById = request.CurrentUserId,
            AddedAt = DateTime.UtcNow
        };

        _context.ProjectMembers.Add(projectMember);

        await _context.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
