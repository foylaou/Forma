using Forma.Application.Common.Interfaces;
using Forma.Application.Features.Organizations.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Organizations.Queries.GetOrganizationById;

/// <summary>
/// 取得組織詳情查詢處理器
/// </summary>
public class GetOrganizationByIdQueryHandler : IRequestHandler<GetOrganizationByIdQuery, OrganizationDto>
{
    private readonly IApplicationDbContext _context;

    public GetOrganizationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<OrganizationDto> Handle(GetOrganizationByIdQuery request, CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations
            .Include(o => o.Projects)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization == null)
        {
            throw new KeyNotFoundException("找不到組織");
        }

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
