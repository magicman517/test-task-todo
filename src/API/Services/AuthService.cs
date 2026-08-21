using System.Security.Claims;
using API.Interfaces;
using API.Models.Requests;
using API.Models.Responses;
using API.Persistence.Entities;
using ErrorOr;
using Microsoft.AspNetCore.Identity;

namespace API.Services;

public class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthService> logger) : IAuthService
{
    private const bool LoginIsPersistent = true;
    private const bool LoginLockoutOnFailure = false;

    public ErrorOr<UserDto> GetCurrentUser()
    {
        var claims = httpContextAccessor.HttpContext?.User;
        if (claims?.Identity is null || !claims.Identity.IsAuthenticated)
        {
            return Error.Unauthorized();
        }

        var userIdStr = claims.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr is null || !Guid.TryParse(userIdStr, out var userId))
        {
            logger.LogWarning("Failed to extract valid user ID from claims");
            return Error.Unauthorized();
        }

        var email = claims.FindFirstValue(ClaimTypes.Email)
                    ?? claims.FindFirstValue(ClaimTypes.Name)
                    ?? string.Empty;

        var response = new UserDto
        {
            Id = userId,
            Email = email
        };

        return response;
    }

    public async Task<ErrorOr<UserDto>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Attempting to register user with email {Email}", request.Email);

        var user = new AppUser
        {
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            logger.LogWarning("Registration failed for email {Email}. Errors: {Errors}",
                request.Email,
                string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}")));

            return result.Errors
                .Select(e => Error.Failure(code: e.Code, description: e.Description))
                .First();
        }

        logger.LogInformation("Successfully registered user {UserId} with email {Email}", user.Id, user.Email);

        return await LoginAsync(new LoginRequest { Email = user.Email, Password = request.Password }, ct);
    }

    public async Task<ErrorOr<UserDto>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Attempting login for user with email {Email}", request.Email);

        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            logger.LogWarning("Login failed: user with email {Email} not found", request.Email);
            return Error.Unauthorized(description: "Invalid credentials");
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            LoginIsPersistent,
            LoginLockoutOnFailure);

        if (!result.Succeeded)
        {
            logger.LogWarning("Login failed for user {UserId} ({Email}): invalid credentials", user.Id, request.Email);
            return Error.Unauthorized(description: "Invalid credentials");
        }

        logger.LogInformation("User {UserId} ({Email}) logged in", user.Id, request.Email);
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email!,
        };
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var claims = httpContextAccessor.HttpContext?.User;
        await signInManager.SignOutAsync();
        logger.LogInformation("User {UserId} logged out", claims?.Identity?.Name);
    }
}
