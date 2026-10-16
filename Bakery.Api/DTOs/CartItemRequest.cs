using System.ComponentModel.DataAnnotations;
using Bakery.Application.Cart;
using Bakery.Domain.Validation;

namespace Bakery.Api.DTOs;

public sealed class CartItemRequest
{
    [NotEmptyGuid]
    public Guid CustomerId { get; init; }

    [NotEmptyGuid]
    public Guid ProductId { get; init; }

    [Range(1, 10000)]
    public int Quantity { get; init; }

    public CartItemInput ToInput() => new(CustomerId, ProductId, Quantity);
}
