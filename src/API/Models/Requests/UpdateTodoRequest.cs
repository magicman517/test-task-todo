using FluentValidation;

namespace API.Models.Requests;

public record UpdateTodoRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }

    public bool IsCompleted { get; init; }

    public Guid? CategoryId { get; init; }
}

public sealed class UpdateTodoRequestValidator : AbstractValidator<UpdateTodoRequest>
{
    public UpdateTodoRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotNull().NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title is too long");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description is too long");
    }
}
