using Bakery.Domain.Entities;

namespace Bakery.Application.Repositories;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Category entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Category entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
