using FluentValidation;

namespace Forma.Application.Features.Projects.Commands.CreateProject;

/// <summary>
/// 建立專案命令驗證器
/// </summary>
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("組織 ID 為必填");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("專案名稱為必填")
            .MaximumLength(200).WithMessage("專案名稱不能超過 200 字元");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("專案代碼為必填")
            .MaximumLength(50).WithMessage("專案代碼不能超過 50 字元")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("專案代碼只能包含英文字母、數字、連字號和底線");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("專案描述不能超過 1000 字元");

        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("年度必須介於 2000 到 2100 之間");

        RuleFor(x => x.Budget)
            .GreaterThanOrEqualTo(0).When(x => x.Budget.HasValue)
            .WithMessage("預算不能為負數");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("結束日期必須晚於或等於開始日期");

        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("使用者 ID 為必填");
    }
}
