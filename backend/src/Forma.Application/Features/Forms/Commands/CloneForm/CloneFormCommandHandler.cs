using Forma.Application.Common.Interfaces;
using Forma.Domain.Entities;
using Forma.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Forms.Commands.CloneForm;

/// <summary>
/// 複製表單命令處理器
/// </summary>
public class CloneFormCommandHandler : IRequestHandler<CloneFormCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CloneFormCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CloneFormCommand request, CancellationToken cancellationToken)
    {
        // 取得來源表單
        var sourceForm = await _context.Forms
            .Include(f => f.Project)
                .ThenInclude(p => p.Members)
            .FirstOrDefaultAsync(f => f.Id == request.FormId, cancellationToken);

        if (sourceForm == null)
        {
            throw new KeyNotFoundException("找不到表單");
        }

        // 權限檢查：必須是來源專案成員或系統管理員
        var isMember = sourceForm.Project.Members.Any(m =>
            m.UserId == request.CurrentUserId && m.RemovedAt == null);

        if (!request.IsSystemAdmin && !isMember)
        {
            throw new UnauthorizedAccessException();
        }

        // 確定目標專案
        var targetProjectId = request.TargetProjectId ?? sourceForm.ProjectId;

        // 如果目標專案不同，檢查目標專案權限
        if (targetProjectId != sourceForm.ProjectId)
        {
            var targetProject = await _context.Projects
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == targetProjectId, cancellationToken);

            if (targetProject == null)
            {
                throw new KeyNotFoundException("找不到目標專案");
            }

            var isTargetMember = targetProject.Members.Any(m =>
                m.UserId == request.CurrentUserId && m.RemovedAt == null);

            if (!request.IsSystemAdmin && !isTargetMember)
            {
                throw new UnauthorizedAccessException("無權在目標專案建立表單");
            }
        }

        // 產生新表單名稱
        var newName = request.NewName;
        if (string.IsNullOrEmpty(newName))
        {
            newName = $"{sourceForm.Name} (複製)";
        }

        // 建立新表單
        var newForm = new Form
        {
            Id = Guid.CreateVersion7(),
            ProjectId = targetProjectId,
            Name = newName,
            Description = sourceForm.Description,
            Schema = sourceForm.Schema,
            TemplateId = sourceForm.TemplateId,
            CreatedById = request.CurrentUserId,
            IsActive = true,
            Version = "1.0",
            AccessControl = sourceForm.AccessControl,
            PublishedAt = null // 複製的表單預設未發布
        };

        _context.Forms.Add(newForm);
        await _context.SaveChangesAsync(cancellationToken);

        return newForm.Id;
    }
}
