using FluentValidation;

namespace Forma.Application.Features.Templates.Commands.UpdateTemplate;

/// <summary>
/// 更新範本命令驗證器
/// </summary>
public class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(x => x.TemplateId)
            .NotEmpty().WithMessage("範本 ID 為必填");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("範本名稱為必填")
            .MaximumLength(200).WithMessage("範本名稱不能超過 200 字元");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("描述不能超過 2000 字元");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("分類不能超過 100 字元");
    }
}
