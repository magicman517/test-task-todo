namespace API.Models.Responses;

public sealed record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
}
