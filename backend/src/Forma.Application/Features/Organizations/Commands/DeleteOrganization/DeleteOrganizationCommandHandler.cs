using Forma.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Organizations.Commands.DeleteOrganization;

/// <summary>
/// 刪除組織命令處理器
/// </summary>
public class DeleteOrganizationCommandHandler : IRequestHandler<DeleteOrganizationCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteOrganizationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteOrganizationCommand request, CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations
            .Include(o => o.Projects)
            .FirstOrDefaultAsync(o => o.Id == request.OrganizationId, cancellationToken);

        if (organization == null)
        {
            throw new KeyNotFoundException("找不到組織");
        }

        // 檢查是否有專案
        if (organization.Projects.Any())
        {
            throw new InvalidOperationException("無法刪除含有專案的組織，請先刪除所有專案");
        }

        _context.Organizations.Remove(organization);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
