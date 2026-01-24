using FluentValidation;

namespace Forma.Application.Features.Auth.Commands.RefreshToken;

/// <summary>
/// 刷新 Token 命令驗證器
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token 為必填");
    }
}
