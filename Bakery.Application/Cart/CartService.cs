using Bakery.Application.DTOs;
using Bakery.Application.Mapping;
using Bakery.Application.Repositories;
using Bakery.Domain.Entities;
using CartEntity = Bakery.Domain.Entities.Cart;

namespace Bakery.Application.Cart;

public sealed class CartService(
    ICartRepository cartRepository,
    IProductRepository productRepository) : ICartService
{
    private const int MaximumItemQuantity = 10000;
    private readonly SemaphoreSlim _mutationLock = new(1, 1);

    public async Task<CartDto> GetAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        ValidateCustomerId(customerId);
        var cart = await cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);
        return (cart ?? CreateCart(customerId)).ToDto();
    }

    public async Task<CartDto?> AddAsync(
        CartItemInput input,
        CancellationToken cancellationToken = default)
    {
        await _mutationLock.WaitAsync(cancellationToken);
        try
        {
            ValidateInput(input);
            var product = await productRepository.GetByIdAsync(input.ProductId, cancellationToken);
            if (product is null)
            {
                return null;
            }

            if (!product.IsAvailable)
            {
                throw new ArgumentException("The product is not currently available.", nameof(input));
            }

            var cart = await cartRepository.GetByCustomerIdAsync(input.CustomerId, cancellationToken)
                ?? CreateCart(input.CustomerId);
            var item = cart.Items.FirstOrDefault(existing => existing.ProductId == input.ProductId);
            if (item is null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = input.Quantity,
                    UnitPrice = product.Price
                });
            }
            else
            {
                var newQuantity = item.Quantity + input.Quantity;
                ValidateQuantity(newQuantity);
                item.Quantity = newQuantity;
                item.ProductName = product.Name;
                item.UnitPrice = product.Price;
            }

            cart.UpdatedAtUtc = DateTime.UtcNow;
            await cartRepository.SaveAsync(cart, cancellationToken);
            return cart.ToDto();
        }
        finally
        {
            _mutationLock.Release();
        }
    }

    public async Task<CartDto?> UpdateAsync(
        CartItemInput input,
        CancellationToken cancellationToken = default)
    {
        await _mutationLock.WaitAsync(cancellationToken);
        try
        {
            ValidateInput(input);
            var cart = await cartRepository.GetByCustomerIdAsync(input.CustomerId, cancellationToken);
            var item = cart?.Items.FirstOrDefault(existing => existing.ProductId == input.ProductId);
            if (cart is null || item is null)
            {
                return null;
            }

            item.Quantity = input.Quantity;
            cart.UpdatedAtUtc = DateTime.UtcNow;
            await cartRepository.SaveAsync(cart, cancellationToken);
            return cart.ToDto();
        }
        finally
        {
            _mutationLock.Release();
        }
    }

    public async Task<CartDto?> RemoveAsync(
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        await _mutationLock.WaitAsync(cancellationToken);
        try
        {
            ValidateCustomerId(customerId);
            if (productId == Guid.Empty)
            {
                throw new ArgumentException("A product identifier is required.", nameof(productId));
            }

            var cart = await cartRepository.GetByCustomerIdAsync(customerId, cancellationToken);
            if (cart is null || cart.Items.RemoveAll(item => item.ProductId == productId) == 0)
            {
                return null;
            }

            cart.UpdatedAtUtc = DateTime.UtcNow;
            await cartRepository.SaveAsync(cart, cancellationToken);
            return cart.ToDto();
        }
        finally
        {
            _mutationLock.Release();
        }
    }

    private static CartEntity CreateCart(Guid customerId) =>
        new() { CustomerId = customerId };

    private static void ValidateInput(CartItemInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        ValidateCustomerId(input.CustomerId);

        if (input.ProductId == Guid.Empty)
        {
            throw new ArgumentException("A product identifier is required.", nameof(input));
        }

        ValidateQuantity(input.Quantity);
    }

    private static void ValidateCustomerId(Guid customerId)
    {
        if (customerId == Guid.Empty)
        {
            throw new ArgumentException("A customer identifier is required.", nameof(customerId));
        }
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity is < 1 or > MaximumItemQuantity)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                $"Quantity must be between 1 and {MaximumItemQuantity}.");
        }
    }
}
