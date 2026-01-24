using FluentValidation;

namespace Forma.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// 管理員更新使用者命令驗證器
/// </summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("使用者 ID 為必填");

        RuleFor(x => x.Username)
            .MinimumLength(3).WithMessage("使用者名稱長度至少需 3 字元")
            .MaximumLength(50).WithMessage("使用者名稱長度不可超過 50 字元")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("使用者名稱只能包含英文字母、數字和底線")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("電子郵件格式不正確")
            .MaximumLength(256).WithMessage("電子郵件長度不可超過 256 字元")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.SystemRole)
            .Must(role => role == null || new[] { "User", "Auditor", "SystemAdmin" }.Contains(role))
            .WithMessage("系統角色必須是 User、Auditor 或 SystemAdmin");

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("部門名稱長度不可超過 100 字元")
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("職稱長度不可超過 100 字元")
            .When(x => !string.IsNullOrEmpty(x.JobTitle));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("電話號碼長度不可超過 20 字元")
            .Matches(@"^[\d\-\+\(\)\s]*$").WithMessage("電話號碼格式不正確")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
