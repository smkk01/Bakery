using Bakery.Domain.Entities;

namespace Bakery.Application.Repositories;

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Order entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Order entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
