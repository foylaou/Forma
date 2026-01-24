using FluentValidation;

namespace Forma.Application.Features.Users.Commands.UpdateProfile;

/// <summary>
/// 更新個人資料命令驗證器
/// </summary>
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
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
