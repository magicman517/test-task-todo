using System.Linq.Expressions;
using API.Persistence.Entities;

namespace API.Models.Responses;

public sealed record CategoryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    public required int PendingTasksCount { get; init; }

    public static Expression<Func<Category, CategoryDto>> Projection =>
        category => new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            PendingTasksCount = category.TodoItems.Count(t => !t.IsComplete)
        };

    public static CategoryDto FromEntity(Category entity) => Projection.Compile().Invoke(entity);
}
