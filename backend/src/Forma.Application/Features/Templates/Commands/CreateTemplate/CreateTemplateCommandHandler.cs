using Forma.Application.Common.Interfaces;
using Forma.Domain.Entities;
using MediatR;

namespace Forma.Application.Features.Templates.Commands.CreateTemplate;

/// <summary>
/// 建立範本命令處理器
/// </summary>
public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new FormTemplate
        {
            Id = Guid.CreateVersion7(),
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Schema = request.Schema,
            ThumbnailUrl = request.ThumbnailUrl,
            IsPublic = request.IsPublic,
            CreatedById = request.CurrentUserId,
            UsageCount = 0
        };

        _context.FormTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return template.Id;
    }
}
