namespace Bakery.Infrastructure.Persistence;

public interface IJsonDatabaseService
{
    Task<T?> ReadAsync<T>(string fileName, CancellationToken cancellationToken = default);

    Task WriteAsync<T>(string fileName, T data, CancellationToken cancellationToken = default);
}
