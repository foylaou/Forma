using FluentValidation;

namespace Forma.Application.Features.Auth.Commands.UpdateProfile;

/// <summary>
/// 更新個人資料命令驗證器
/// </summary>
public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("使用者 ID 為必填");

        RuleFor(x => x.Department)
            .MaximumLength(100).WithMessage("部門不能超過 100 字元");

        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("職稱不能超過 100 字元");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("電話號碼不能超過 20 字元")
            .Matches(@"^[\d\-\+\(\)\s]*$").WithMessage("電話號碼格式不正確")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
