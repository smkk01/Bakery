using Bakery.Application.Authentication;
using Bakery.Application.Repositories;
using Bakery.Domain.Entities;
using Bakery.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Bakery.Api.Authentication;

public sealed class AdminBootstrapper(
    IConfiguration configuration,
    ICustomerRepository customers,
    IPasswordHasher passwordHasher,
    ILogger<AdminBootstrapper> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var promotionEmail = configuration["AdminPromotion:Email"];
        if (!string.IsNullOrWhiteSpace(promotionEmail))
        {
            await PromoteExistingCustomerAsync(promotionEmail, cancellationToken);
            return;
        }

        var fullName = configuration["BootstrapAdmin:FullName"];
        var email = configuration["BootstrapAdmin:Email"];
        var phone = configuration["BootstrapAdmin:Phone"];
        var password = configuration["BootstrapAdmin:Password"];
        var configuredValues = new[] { fullName, email, phone, password };

        if (configuredValues.All(string.IsNullOrWhiteSpace))
        {
            return;
        }

        if (configuredValues.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                "BootstrapAdmin FullName, Email, Phone, and Password must all be configured together.");
        }

        if (password!.Length < 12 || password.Length > 128 ||
            !password.Any(char.IsLetter) ||
            !password.Any(char.IsDigit))
        {
            throw new InvalidOperationException(
                "Bootstrap admin password must be 12-128 characters and include at least one letter and one digit.");
        }

        if (fullName!.Trim().Length is < 1 or > 200 ||
            !new EmailAddressAttribute().IsValid(email) ||
            email!.Length > 254 ||
            phone!.Trim().Length is < 1 or > 32 ||
            !new PhoneAttribute().IsValid(phone))
        {
            throw new InvalidOperationException(
                "Bootstrap admin name, email, or phone is invalid.");
        }

        var normalizedEmail = email!.Trim().ToLowerInvariant();
        var existingCustomer = (await customers.GetAllAsync(cancellationToken))
            .FirstOrDefault(customer =>
                string.Equals(customer.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));

        if (existingCustomer is not null)
        {
            if (existingCustomer.Role != UserRole.Admin)
            {
                throw new InvalidOperationException(
                    "The configured bootstrap admin email already belongs to a non-admin account.");
            }

            return;
        }

        var admin = new Customer
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            Phone = phone!.Trim(),
            PasswordHash = passwordHasher.HashPassword(password),
            Role = UserRole.Admin
        };

        await customers.CreateAsync(admin, cancellationToken);
        logger.LogInformation("Bootstrapped administrator account {AdminId}", admin.Id);
    }

    private async Task PromoteExistingCustomerAsync(
        string email,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (normalizedEmail.Length > 254 ||
            !new EmailAddressAttribute().IsValid(normalizedEmail))
        {
            throw new InvalidOperationException("AdminPromotion email is invalid.");
        }

        var customer = (await customers.GetAllAsync(cancellationToken))
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase));

        if (customer is null)
        {
            throw new InvalidOperationException(
                "AdminPromotion email does not match an existing customer account.");
        }

        if (customer.Role == UserRole.Admin)
        {
            logger.LogInformation("Account {CustomerId} is already an administrator", customer.Id);
            return;
        }

        customer.Role = UserRole.Admin;
        customer.UpdatedAtUtc = DateTime.UtcNow;
        if (!await customers.UpdateAsync(customer, cancellationToken))
        {
            throw new InvalidOperationException(
                "The customer account could not be updated to administrator.");
        }

        logger.LogWarning("Promoted customer account {CustomerId} to administrator", customer.Id);
    }
}
