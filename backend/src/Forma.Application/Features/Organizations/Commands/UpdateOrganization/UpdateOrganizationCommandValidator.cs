using FluentValidation;
using Forma.Domain.Enums;

namespace Forma.Application.Features.Organizations.Commands.UpdateOrganization;

/// <summary>
/// 更新組織命令驗證器
/// </summary>
public class UpdateOrganizationCommandValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("組織 ID 為必填");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("組織名稱為必填")
            .MaximumLength(200).WithMessage("組織名稱不能超過 200 字元");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("描述不能超過 500 字元");

        RuleFor(x => x.Type)
            .Must(t => string.IsNullOrEmpty(t) || Enum.TryParse<OrganizationType>(t, true, out _))
            .WithMessage("無效的組織類型，有效值為: Central, Local");
    }
}
