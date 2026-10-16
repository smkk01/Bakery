using System.ComponentModel.DataAnnotations;
using Bakery.Application.Authentication;

namespace Bakery.Api.DTOs;

public sealed class LoginRequest
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 1)]
    public string Password { get; init; } = string.Empty;

    public LoginRequestDto ToApplicationDto() => new(Email, Password);
}
