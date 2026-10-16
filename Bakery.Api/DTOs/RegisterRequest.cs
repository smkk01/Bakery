using System.ComponentModel.DataAnnotations;
using Bakery.Application.Authentication;

namespace Bakery.Api.DTOs;

public sealed class RegisterRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required, Phone, StringLength(32)]
    public string Phone { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    public string Password { get; init; } = string.Empty;

    public RegisterRequestDto ToApplicationDto() =>
        new(FullName, Email, Phone, Password);
}
