using API.Persistence.Common;

namespace API.Persistence.Entities;

public sealed class TodoItem : IEntityWithTimestamps
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsComplete { get; set; } = false;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public Guid? CategoryId { get; set; }
    public Category? Category { get; set; }
}
