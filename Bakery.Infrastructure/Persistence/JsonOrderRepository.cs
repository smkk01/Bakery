using Bakery.Application.Repositories;
using Bakery.Domain.Entities;

namespace Bakery.Infrastructure.Persistence;

public sealed class JsonOrderRepository(IJsonDatabaseService database)
    : JsonFileRepository<Order>(database, "orders.json"), IOrderRepository
{
    public Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllEntitiesAsync(cancellationToken);

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetEntityByIdAsync(id, cancellationToken);

    public Task CreateAsync(Order entity, CancellationToken cancellationToken = default) =>
        CreateEntityAsync(entity, cancellationToken);

    public Task<bool> UpdateAsync(Order entity, CancellationToken cancellationToken = default) =>
        UpdateEntityAsync(entity, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        DeleteEntityAsync(id, cancellationToken);
}
