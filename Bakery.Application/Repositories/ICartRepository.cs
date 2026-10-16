using Bakery.Domain.Entities;
using CartEntity = Bakery.Domain.Entities.Cart;

namespace Bakery.Application.Repositories;

public interface ICartRepository
{
    Task<CartEntity?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task SaveAsync(CartEntity cart, CancellationToken cancellationToken = default);
}
