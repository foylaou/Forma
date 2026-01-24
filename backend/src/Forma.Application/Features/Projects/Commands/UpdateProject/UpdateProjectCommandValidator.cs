using FluentValidation;
using Forma.Domain.Enums;

namespace Forma.Application.Features.Projects.Commands.UpdateProject;

/// <summary>
/// 更新專案命令驗證器
/// </summary>
public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("專案 ID 為必填");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("專案名稱為必填")
            .MaximumLength(200).WithMessage("專案名稱不能超過 200 字元");

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

        RuleFor(x => x.Status)
            .Must(status => string.IsNullOrEmpty(status) || Enum.TryParse<ProjectStatus>(status, true, out _))
            .WithMessage("無效的專案狀態");

        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("使用者 ID 為必填");
    }
}
