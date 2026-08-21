using FluentValidation;

namespace API.Models.Requests;

public sealed record UpdateCategoryRequest
{
    public required string Name { get; init; }
}

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Category name is required")
            .MaximumLength(50).WithMessage("Category name cannot exceed 50 characters");
    }
}
