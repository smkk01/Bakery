using Bakery.Api.Authentication;
using Bakery.Application.Authentication;
using Bakery.Application.Repositories;
using Bakery.Domain.Entities;
using Bakery.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bakery.Tests.Authentication;

public sealed class AdminBootstrapperTests
{
    [Fact]
    public async Task InitializeAsync_PromotionEmailPromotesExistingCustomerAndPreservesPassword()
    {
        var customer = new Customer
        {
            FullName = "Alex Baker",
            Email = "alex@example.com",
            Phone = "+15551234567",
            PasswordHash = "existing-hash",
            Role = UserRole.Customer
        };
        var repository = new InMemoryCustomerRepository(customer);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminPromotion:Email"] = " ALEX@example.com "
            })
            .Build();
        var bootstrapper = new AdminBootstrapper(
            configuration,
            repository,
            new UnusedPasswordHasher(),
            NullLogger<AdminBootstrapper>.Instance);

        await bootstrapper.InitializeAsync();

        Assert.Equal(UserRole.Admin, customer.Role);
        Assert.Equal("existing-hash", customer.PasswordHash);
        Assert.Equal(1, repository.UpdateCount);
        Assert.Empty(repository.CreatedCustomers);
    }

    [Fact]
    public async Task InitializeAsync_PromotionEmailForUnknownCustomerThrows()
    {
        var repository = new InMemoryCustomerRepository();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminPromotion:Email"] = "missing@example.com"
            })
            .Build();
        var bootstrapper = new AdminBootstrapper(
            configuration,
            repository,
            new UnusedPasswordHasher(),
            NullLogger<AdminBootstrapper>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() => bootstrapper.InitializeAsync());
    }

    private sealed class InMemoryCustomerRepository(params Customer[] customers) : ICustomerRepository
    {
        private readonly List<Customer> _customers = [.. customers];

        public int UpdateCount { get; private set; }

        public List<Customer> CreatedCustomers { get; } = [];

        public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Customer>>(_customers);

        public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_customers.FirstOrDefault(customer => customer.Id == id));

        public Task CreateAsync(Customer entity, CancellationToken cancellationToken = default)
        {
            CreatedCustomers.Add(entity);
            _customers.Add(entity);
            return Task.CompletedTask;
        }

        public Task<bool> UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
        {
            UpdateCount++;
            return Task.FromResult(_customers.Any(customer => customer.Id == entity.Id));
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
    }

    private sealed class UnusedPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) =>
            throw new NotSupportedException("Password hashing is not used during customer promotion.");

        public bool VerifyPassword(string password, string passwordHash) =>
            throw new NotSupportedException("Password verification is not used during customer promotion.");
    }
}
