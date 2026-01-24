using Forma.Application.Common.Interfaces;
using Forma.Domain.Entities;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Forms.Commands.CreateForm;

/// <summary>
/// 建立表單命令處理器
/// </summary>
public class CreateFormCommandHandler : IRequestHandler<CreateFormCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateFormCommand request, CancellationToken cancellationToken)
    {
        // 檢查專案是否存在
        var project = await _context.Projects
            .Include(p => p.Members.Where(m => m.RemovedAt == null))
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, cancellationToken);

        if (project == null)
        {
            throw new KeyNotFoundException("找不到專案");
        }

        // 檢查權限：必須是專案成員
        var userMember = project.Members
            .FirstOrDefault(m => m.UserId == request.CurrentUserId);

        if (userMember == null && !request.IsSystemAdmin)
        {
            throw new UnauthorizedAccessException("您沒有權限在此專案建立表單");
        }

        // 如果有指定範本，驗證範本是否存在
        if (request.TemplateId.HasValue)
        {
            var templateExists = await _context.FormTemplates
                .AnyAsync(t => t.Id == request.TemplateId.Value, cancellationToken);

            if (!templateExists)
            {
                throw new KeyNotFoundException("找不到表單範本");
            }
        }

        // 解析存取控制
        var accessControl = Enum.Parse<FormAccessControl>(request.AccessControl, true);

        // 建立表單
        var form = new Form
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            Name = request.Name,
            Description = request.Description,
            Schema = request.Schema,
            TemplateId = request.TemplateId,
            AccessControl = accessControl,
            CreatedById = request.CurrentUserId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Version = "1.0"
        };

        _context.Forms.Add(form);
        await _context.SaveChangesAsync(cancellationToken);

        return form.Id;
    }
}
