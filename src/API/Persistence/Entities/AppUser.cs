using API.Persistence.Common;
using Microsoft.AspNetCore.Identity;

namespace API.Persistence.Entities;

public sealed class AppUser : IdentityUser<Guid>, IEntityWithTimestamps
{
    public override Guid Id { get; set; } = Guid.CreateVersion7();
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}