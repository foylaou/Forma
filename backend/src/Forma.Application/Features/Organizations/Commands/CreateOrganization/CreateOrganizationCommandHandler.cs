using Forma.Application.Common.Interfaces;
using Forma.Domain.Entities;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Organizations.Commands.CreateOrganization;

/// <summary>
/// 建立組織命令處理器
/// </summary>
public class CreateOrganizationCommandHandler : IRequestHandler<CreateOrganizationCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateOrganizationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateOrganizationCommand request, CancellationToken cancellationToken)
    {
        // 檢查組織代碼是否已存在
        var codeExists = await _context.Organizations
            .AnyAsync(o => o.Code == request.Code, cancellationToken);

        if (codeExists)
        {
            throw new InvalidOperationException("組織代碼已存在");
        }

        var type = Enum.Parse<OrganizationType>(request.Type, true);

        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };

        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync(cancellationToken);

        return organization.Id;
    }
}
