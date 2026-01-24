using Forma.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Forma.Application.Features.Templates.Commands.DeleteTemplate;

/// <summary>
/// 刪除範本命令處理器
/// </summary>
public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand, Unit>
{
    private readonly IApplicationDbContext _context;

    public DeleteTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.FormTemplates
            .Include(t => t.Forms)
            .FirstOrDefaultAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (template == null)
        {
            throw new KeyNotFoundException("找不到範本");
        }

        // 權限檢查：建立者或系統管理員
        if (!request.IsSystemAdmin && template.CreatedById != request.CurrentUserId)
        {
            throw new UnauthorizedAccessException();
        }

        // 檢查是否有表單使用此範本
        if (template.Forms.Any())
        {
            throw new InvalidOperationException("此範本已被表單使用，無法刪除");
        }

        _context.FormTemplates.Remove(template);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
