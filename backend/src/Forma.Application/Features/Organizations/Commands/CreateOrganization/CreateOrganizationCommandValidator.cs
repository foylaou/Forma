using FluentValidation;
using Forma.Domain.Enums;

namespace Forma.Application.Features.Organizations.Commands.CreateOrganization;

/// <summary>
/// 建立組織命令驗證器
/// </summary>
public class CreateOrganizationCommandValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("組織名稱為必填")
            .MaximumLength(200).WithMessage("組織名稱不能超過 200 字元");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("組織代碼為必填")
            .MaximumLength(50).WithMessage("組織代碼不能超過 50 字元")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("組織代碼只能包含英文字母、數字、連字號和底線");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("描述不能超過 500 字元");

        RuleFor(x => x.Type)
            .Must(t => Enum.TryParse<OrganizationType>(t, true, out _))
            .WithMessage("無效的組織類型，有效值為: Central, Local");
    }
}
