using Bakery.Application.Repositories;
using Bakery.Domain.Entities;

namespace Bakery.Infrastructure.Persistence;

public sealed class JsonProductRepository(IJsonDatabaseService database)
    : JsonFileRepository<Product>(database, "products.json"), IProductRepository
{
    public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllEntitiesAsync(cancellationToken);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetEntityByIdAsync(id, cancellationToken);

    public Task CreateAsync(Product entity, CancellationToken cancellationToken = default) =>
        CreateEntityAsync(entity, cancellationToken);

    public Task<bool> UpdateAsync(Product entity, CancellationToken cancellationToken = default) =>
        UpdateEntityAsync(entity, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        DeleteEntityAsync(id, cancellationToken);
}
