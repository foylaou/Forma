using FluentValidation;

namespace Forma.Application.Features.Forms.Commands.CloneForm;

/// <summary>
/// 複製表單命令驗證器
/// </summary>
public class CloneFormCommandValidator : AbstractValidator<CloneFormCommand>
{
    public CloneFormCommandValidator()
    {
        RuleFor(x => x.FormId)
            .NotEmpty().WithMessage("表單 ID 為必填");

        RuleFor(x => x.NewName)
            .MaximumLength(200).WithMessage("表單名稱不能超過 200 字元")
            .When(x => !string.IsNullOrEmpty(x.NewName));
    }
}
