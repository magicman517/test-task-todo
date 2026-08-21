namespace API.Persistence.Common;

public interface IEntityWithTimestamps
{
    DateTimeOffset CreatedAt { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}