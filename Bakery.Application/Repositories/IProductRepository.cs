using Bakery.Domain.Entities;

namespace Bakery.Application.Repositories;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Product entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Product entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
