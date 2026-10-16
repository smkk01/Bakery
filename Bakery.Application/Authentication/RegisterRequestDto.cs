namespace Bakery.Application.Authentication;

public sealed record RegisterRequestDto(
    string FullName,
    string Email,
    string Phone,
    string Password);
