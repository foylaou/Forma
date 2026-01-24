using FluentValidation;
using Forma.Domain.Enums;

namespace Forma.Application.Features.Projects.Commands.AddProjectMember;

/// <summary>
/// 新增專案成員命令驗證器
/// </summary>
public class AddProjectMemberCommandValidator : AbstractValidator<AddProjectMemberCommand>
{
    public AddProjectMemberCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("專案 ID 為必填");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("使用者 ID 為必填");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("專案角色為必填")
            .Must(role => Enum.TryParse<ProjectRole>(role, true, out _))
            .WithMessage("無效的專案角色，有效值為: Member, Manager, Owner");

        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("當前使用者 ID 為必填");
    }
}
