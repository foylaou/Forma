using FluentValidation;

namespace Forma.Application.Features.Auth.Commands.Register;

/// <summary>
/// 註冊命令驗證器
/// </summary>
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("使用者名稱為必填")
            .MinimumLength(3).WithMessage("使用者名稱長度至少需 3 字元")
            .MaximumLength(50).WithMessage("使用者名稱長度不可超過 50 字元")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("使用者名稱只能包含英文字母、數字和底線");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("電子郵件為必填")
            .EmailAddress().WithMessage("電子郵件格式不正確")
            .MaximumLength(256).WithMessage("電子郵件長度不可超過 256 字元");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼為必填")
            .MinimumLength(8).WithMessage("密碼長度至少需 8 字元")
            .MaximumLength(100).WithMessage("密碼長度不可超過 100 字元")
            .Matches(@"[A-Z]").WithMessage("密碼必須包含至少一個大寫字母")
            .Matches(@"[a-z]").WithMessage("密碼必須包含至少一個小寫字母")
            .Matches(@"[0-9]").WithMessage("密碼必須包含至少一個數字");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("確認密碼為必填")
            .Equal(x => x.Password).WithMessage("確認密碼與密碼不符");

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("部門名稱長度不可超過 100 字元")
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("職稱長度不可超過 100 字元")
            .When(x => !string.IsNullOrEmpty(x.JobTitle));
    }
}
