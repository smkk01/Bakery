using Bakery.Application.Repositories;
using Bakery.Domain.Entities;

namespace Bakery.Infrastructure.Persistence;

public sealed class JsonCartRepository(IJsonDatabaseService database) : ICartRepository
{
    private const string FileName = "carts.json";
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public async Task<Cart?> GetByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var carts = await ReadAllAsync(cancellationToken);
            return carts.FirstOrDefault(cart => cart.CustomerId == customerId);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task SaveAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cart);
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            var carts = await ReadAllAsync(cancellationToken);
            var index = carts.FindIndex(existing => existing.CustomerId == cart.CustomerId);
            if (index < 0)
            {
                carts.Add(cart);
            }
            else
            {
                carts[index] = cart;
            }

            await database.WriteAsync(FileName, carts, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<Cart>> ReadAllAsync(CancellationToken cancellationToken) =>
        await database.ReadAsync<List<Cart>>(FileName, cancellationToken) ?? [];
}
