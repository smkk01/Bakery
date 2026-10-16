using Bakery.Application.DTOs;

namespace Bakery.Application.Cart;

public interface ICartService
{
    Task<CartDto> GetAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task<CartDto?> AddAsync(CartItemInput input, CancellationToken cancellationToken = default);

    Task<CartDto?> UpdateAsync(CartItemInput input, CancellationToken cancellationToken = default);

    Task<CartDto?> RemoveAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default);
}
