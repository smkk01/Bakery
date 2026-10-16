namespace Bakery.Application.Authentication;

public sealed record AuthResultDto(
    Guid UserId,
    string FullName,
    string Email,
    string Role,
    string AccessToken,
    DateTime ExpiresAtUtc);
