using FluentValidation;
using JetBrains.Annotations;

namespace API.Models.Requests;

[UsedImplicitly]
public sealed record RegisterRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

[UsedImplicitly]
public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotNull().NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotNull().NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password is too short");
    }
}
