using API.Persistence.Common;

namespace API.Persistence.Entities;

public sealed class Category : IEntityWithTimestamps
{
    public Guid Id { get; init; } = Guid.CreateVersion7();
    public required string Name { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public required Guid UserId { get; set; }
    public AppUser User { get; set; } = null!;

    public ICollection<TodoItem> TodoItems { get; set; } = [];
}
