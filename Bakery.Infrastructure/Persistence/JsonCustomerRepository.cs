using Bakery.Application.Repositories;
using Bakery.Domain.Entities;

namespace Bakery.Infrastructure.Persistence;

public sealed class JsonCustomerRepository(IJsonDatabaseService database)
    : JsonFileRepository<Customer>(database, "customers.json"), ICustomerRepository
{
    public Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllEntitiesAsync(cancellationToken);

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetEntityByIdAsync(id, cancellationToken);

    public Task CreateAsync(Customer entity, CancellationToken cancellationToken = default) =>
        CreateEntityAsync(entity, cancellationToken);

    public Task<bool> UpdateAsync(Customer entity, CancellationToken cancellationToken = default) =>
        UpdateEntityAsync(entity, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        DeleteEntityAsync(id, cancellationToken);
}
