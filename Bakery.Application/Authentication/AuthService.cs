using Bakery.Application.Repositories;
using Bakery.Domain.Entities;
using Bakery.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Bakery.Application.Authentication;

public sealed class AuthService(
    ICustomerRepository customerRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly SemaphoreSlim _registrationLock = new(1, 1);

    public async Task<AuthResultDto> RegisterAsync(
        RegisterRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRegistration(request);
        var email = NormalizeEmail(request.Email);

        await _registrationLock.WaitAsync(cancellationToken);
        try
        {
            var customers = await customerRepository.GetAllAsync(cancellationToken);
            if (customers.Any(customer =>
                    string.Equals(customer.Email, email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new EmailAlreadyRegisteredException();
            }

            var customer = new Customer
            {
                FullName = request.FullName.Trim(),
                Email = email,
                Phone = request.Phone.Trim(),
                PasswordHash = passwordHasher.HashPassword(request.Password),
                Role = UserRole.Customer
            };

            await customerRepository.CreateAsync(customer, cancellationToken);
            logger.LogInformation("Registered customer account {CustomerId}", customer.Id);
            return CreateResult(customer);
        }
        finally
        {
            _registrationLock.Release();
        }
    }

    public async Task<AuthResultDto?> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return null;
        }

        var email = NormalizeEmail(request.Email);
        var customers = await customerRepository.GetAllAsync(cancellationToken);
        var customer = customers.FirstOrDefault(candidate =>
            string.Equals(candidate.Email, email, StringComparison.OrdinalIgnoreCase));

        if (customer is null ||
            string.IsNullOrWhiteSpace(customer.PasswordHash) ||
            !passwordHasher.VerifyPassword(request.Password, customer.PasswordHash))
        {
            logger.LogWarning("Failed login attempt for {Email}", email);
            return null;
        }

        logger.LogInformation("Authenticated account {CustomerId}", customer.Id);
        return CreateResult(customer);
    }

    private AuthResultDto CreateResult(Customer customer)
    {
        var (token, expiresAtUtc) = tokenService.CreateToken(customer);
        return new AuthResultDto(
            customer.Id,
            customer.FullName,
            customer.Email,
            customer.Role.ToString(),
            token,
            expiresAtUtc);
    }

    private static void ValidateRegistration(RegisterRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            request.FullName.Trim().Length > 200)
        {
            throw new ArgumentException("Full name is required and must not exceed 200 characters.", nameof(request));
        }

        if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(request.Email) ||
            request.Email.Length > 254)
        {
            throw new ArgumentException("A valid email address is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Phone) || request.Phone.Trim().Length > 32)
        {
            throw new ArgumentException("Phone is required and must not exceed 32 characters.", nameof(request));
        }

        if (string.IsNullOrEmpty(request.Password) ||
            request.Password.Length < 12 || request.Password.Length > 128 ||
            !request.Password.Any(char.IsLetter) ||
            !request.Password.Any(char.IsDigit))
        {
            throw new ArgumentException(
                "Password must be 12-128 characters and include at least one letter and one digit.",
                nameof(request));
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
