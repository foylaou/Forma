using FluentValidation;
using Forma.Domain.Enums;

namespace Forma.Application.Features.Forms.Commands.CreateForm;

/// <summary>
/// 建立表單命令驗證器
/// </summary>
public class CreateFormCommandValidator : AbstractValidator<CreateFormCommand>
{
    public CreateFormCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("專案 ID 為必填");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("表單名稱為必填")
            .MaximumLength(200).WithMessage("表單名稱不能超過 200 字元");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("描述不能超過 1000 字元");

        RuleFor(x => x.Schema)
            .NotEmpty().WithMessage("表單結構定義為必填")
            .Must(BeValidJson).WithMessage("表單結構定義必須是有效的 JSON");

        RuleFor(x => x.AccessControl)
            .Must(ac => Enum.TryParse<FormAccessControl>(ac, true, out _))
            .WithMessage("無效的存取控制設定");

        RuleFor(x => x.CurrentUserId)
            .NotEmpty().WithMessage("使用者 ID 為必填");
    }

    private bool BeValidJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
