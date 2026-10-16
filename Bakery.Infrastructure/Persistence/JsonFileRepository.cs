using Bakery.Domain.Entities;

namespace Bakery.Infrastructure.Persistence;

public abstract class JsonFileRepository<TEntity>(
    IJsonDatabaseService database,
    string fileName)
    where TEntity : BaseEntity
{
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    protected async Task<IReadOnlyList<TEntity>> GetAllEntitiesAsync(
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            return await ReadAllAsync(cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    protected async Task<TEntity?> GetEntityByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var entities = await ReadAllAsync(cancellationToken);
            return entities.FirstOrDefault(entity => entity.Id == id);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    protected async Task CreateEntityAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var entities = await ReadAllAsync(cancellationToken);
            entities.Add(entity);
            await WriteAllAsync(entities, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    protected async Task<bool> UpdateEntityAsync(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var entities = await ReadAllAsync(cancellationToken);
            var index = entities.FindIndex(existing => existing.Id == entity.Id);
            if (index < 0)
            {
                return false;
            }

            entities[index] = entity;
            await WriteAllAsync(entities, cancellationToken);
            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    protected async Task<bool> DeleteEntityAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var entities = await ReadAllAsync(cancellationToken);
            var removed = entities.RemoveAll(entity => entity.Id == id) > 0;
            if (removed)
            {
                await WriteAllAsync(entities, cancellationToken);
            }

            return removed;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<TEntity>> ReadAllAsync(CancellationToken cancellationToken) =>
        await database.ReadAsync<List<TEntity>>(fileName, cancellationToken) ?? [];

    private Task WriteAllAsync(List<TEntity> entities, CancellationToken cancellationToken) =>
        database.WriteAsync(fileName, entities, cancellationToken);
}
