using Bakery.Application.DTOs;
using Bakery.Domain.Entities;
using CartEntity = Bakery.Domain.Entities.Cart;

namespace Bakery.Application.Mapping;

public static class CartMappingExtensions
{
    public static CartDto ToDto(this CartEntity cart) =>
        new(
            cart.Id,
            cart.CustomerId,
            cart.Items.Select(item => item.ToDto()).ToArray(),
            cart.Subtotal,
            cart.Total);

    public static CartItemDto ToDto(this CartItem item) =>
        new(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice, item.Subtotal);
}
