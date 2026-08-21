using FluentValidation;

namespace API.Models.Requests;

public record GetTodosQuery
{
    public string? Search { get; init; } = null;
    public Guid? CategoryId { get; init; } = null;
    public bool? IsCompleted { get; init; } = null;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed class GetTodosQueryValidator : AbstractValidator<GetTodosQuery>
{
    public GetTodosQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page cannot be zero or negative");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page Size cannot be zero or negative");
    }
}
