using FluentValidation;

namespace Forma.Application.Features.Auth.Commands.Login;

/// <summary>
/// 登入命令驗證器
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty().WithMessage("電子郵件或使用者名稱為必填")
            .MaximumLength(256).WithMessage("電子郵件或使用者名稱長度不可超過 256 字元");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密碼為必填")
            .MinimumLength(6).WithMessage("密碼長度至少需 6 字元");
    }
}
