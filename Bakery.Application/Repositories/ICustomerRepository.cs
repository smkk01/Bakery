using Bakery.Domain.Entities;

namespace Bakery.Application.Repositories;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Customer entity, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(Customer entity, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
