using Bakery.Application.Repositories;
using Bakery.Domain.Entities;

namespace Bakery.Infrastructure.Persistence;

public sealed class JsonCategoryRepository(IJsonDatabaseService database)
    : JsonFileRepository<Category>(database, "categories.json"), ICategoryRepository
{
    public Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        GetAllEntitiesAsync(cancellationToken);

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        GetEntityByIdAsync(id, cancellationToken);

    public Task CreateAsync(Category entity, CancellationToken cancellationToken = default) =>
        CreateEntityAsync(entity, cancellationToken);

    public Task<bool> UpdateAsync(Category entity, CancellationToken cancellationToken = default) =>
        UpdateEntityAsync(entity, cancellationToken);

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        DeleteEntityAsync(id, cancellationToken);
}
