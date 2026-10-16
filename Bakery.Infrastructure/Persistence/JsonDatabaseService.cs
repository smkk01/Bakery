using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bakery.Infrastructure.Persistence;

public sealed class JsonDatabaseService : IJsonDatabaseService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private static readonly HashSet<string> AllowedFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "products.json",
        "categories.json",
        "orders.json",
        "customers.json",
        "carts.json"
    };

    private static readonly string[] DatabaseFiles =
    [
        "products.json",
        "categories.json",
        "orders.json",
        "customers.json",
        "carts.json"
    ];

    private readonly string _directoryPath;
    private readonly ILogger<JsonDatabaseService> _logger;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public JsonDatabaseService(
        IOptions<JsonDatabaseOptions> options,
        ILogger<JsonDatabaseService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Value.DirectoryPath);

        _directoryPath = Path.GetFullPath(options.Value.DirectoryPath);
        _logger = logger;
    }

    public async Task<T?> ReadAsync<T>(string fileName, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(fileName);
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            await EnsureFilesExistAsync(cancellationToken);

            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 16 * 1024,
                useAsync: true);

            try
            {
                var result = await JsonSerializer.DeserializeAsync<T>(
                    stream,
                    SerializerOptions,
                    cancellationToken);
                _logger.LogDebug("Read JSON database file {FileName}", fileName);
                return result;
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "JSON database file {FileName} contains invalid JSON", fileName);
                throw;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is not JsonException)
        {
            _logger.LogError(exception, "Failed to read JSON database file {FileName}", fileName);
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task WriteAsync<T>(
        string fileName,
        T data,
        CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(fileName);
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            await EnsureFilesExistAsync(cancellationToken);
            await WriteFileAsync(filePath, data, cancellationToken);
            _logger.LogDebug("Wrote JSON database file {FileName}", fileName);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to write JSON database file {FileName}", fileName);
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private string GetFilePath(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (!AllowedFiles.Contains(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Only products.json, categories.json, orders.json, customers.json, and carts.json are supported.",
                nameof(fileName));
        }

        var canonicalName = DatabaseFiles.First(name =>
            string.Equals(name, fileName, StringComparison.OrdinalIgnoreCase));
        return Path.Combine(_directoryPath, canonicalName);
    }

    private async Task EnsureFilesExistAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_directoryPath);
        foreach (var fileName in DatabaseFiles)
        {
            var filePath = Path.Combine(_directoryPath, fileName);
            if (File.Exists(filePath))
            {
                continue;
            }

            await WriteFileAsync(filePath, Array.Empty<object>(), cancellationToken);
            _logger.LogInformation("Created empty JSON database file {FileName}", fileName);
        }
    }

    private static async Task WriteFileAsync<T>(
        string filePath,
        T data,
        CancellationToken cancellationToken)
    {
        var temporaryPath = $"{filePath}.{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 16 * 1024,
                useAsync: true))
            {
                await JsonSerializer.SerializeAsync(stream, data, SerializerOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }

            File.Move(temporaryPath, filePath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }
}
