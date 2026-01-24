using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Organizations.DTOs;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Organizations.Commands.UpdateOrganization;

/// <summary>
/// 更新組織命令處理器
/// </summary>
public class UpdateOrganizationCommandHandler : IRequestHandler<UpdateOrganizationCommand, OrganizationDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrganizationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrganizationDto> Handle(UpdateOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations
            .Include(o => o.Projects)
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization == null)
        {
            throw new KeyNotFoundException("找不到組織");
        }

        organization.Name = request.Name;
        organization.Description = request.Description;
        organization.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(request.Type) &&
            Enum.TryParse<OrganizationType>(request.Type, true, out var type))
        {
            organization.Type = type;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new OrganizationDto
        {
            Id = organization.Id,
            Name = organization.Name,
            Code = organization.Code,
            Description = organization.Description,
            Type = organization.Type.ToString(),
            CreatedAt = organization.CreatedAt,
            UpdatedAt = organization.UpdatedAt,
            ProjectCount = organization.Projects.Count
        };
    }
}
