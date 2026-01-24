using FluentValidation;

namespace Forma.Application.Features.Submissions.Commands.CreateSubmission;

/// <summary>
/// 建立表單提交命令驗證器
/// </summary>
public class CreateSubmissionCommandValidator : AbstractValidator<CreateSubmissionCommand>
{
    public CreateSubmissionCommandValidator()
    {
        RuleFor(x => x.FormId)
            .NotEmpty().WithMessage("表單 ID 為必填");

        RuleFor(x => x.SubmissionData)
            .NotEmpty().WithMessage("提交資料為必填");
    }
}
