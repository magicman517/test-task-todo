using API.Models.Requests;
using API.Models.Responses;
using ErrorOr;

namespace API.Interfaces;

public interface IAuthService
{
    ErrorOr<UserDto> GetCurrentUser();

    Task<ErrorOr<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<ErrorOr<UserDto>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
}
