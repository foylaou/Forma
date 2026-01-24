using FluentValidation;

namespace Forma.Application.Features.Users.Commands.ChangePassword;

/// <summary>
/// 修改密碼命令驗證器
/// </summary>
public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("目前密碼為必填");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("新密碼為必填")
            .MinimumLength(8).WithMessage("新密碼長度至少需 8 字元")
            .MaximumLength(100).WithMessage("新密碼長度不可超過 100 字元")
            .Matches(@"[A-Z]").WithMessage("新密碼必須包含至少一個大寫字母")
            .Matches(@"[a-z]").WithMessage("新密碼必須包含至少一個小寫字母")
            .Matches(@"[0-9]").WithMessage("新密碼必須包含至少一個數字")
            .NotEqual(x => x.CurrentPassword).WithMessage("新密碼不可與目前密碼相同");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("確認新密碼為必填")
            .Equal(x => x.NewPassword).WithMessage("確認新密碼與新密碼不符");
    }
}
