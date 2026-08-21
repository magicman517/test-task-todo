using System.Linq.Expressions;
using API.Persistence.Entities;

namespace API.Models.Responses;

public record TodoDto
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }
    public string? Description { get; init; }

    public bool IsCompleted { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }

    public static Expression<Func<TodoItem, TodoDto>> Projection =>
        todo => new TodoDto
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsComplete,
            CreatedAt = todo.CreatedAt,
            UpdatedAt = todo.UpdatedAt,
            CategoryId = todo.CategoryId,
            CategoryName = todo.Category != null ? todo.Category.Name : null
        };

    public static TodoDto FromEntity(TodoItem entity) => Projection.Compile().Invoke(entity);
}
