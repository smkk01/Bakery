using System.ComponentModel.DataAnnotations;
using Bakery.Application.DTOs;
using Bakery.Domain.Validation;

namespace Bakery.Api.DTOs;

public sealed class CreateOrderRequest
{
    [NotEmptyGuid]
    public Guid CustomerId { get; init; }

    [Required, MinLength(1)]
    public List<CreateOrderItemRequest> Items { get; init; } = [];

    public CreateOrderRequestDto ToApplicationDto() =>
        new(CustomerId, Items.Select(item => item.ToApplicationDto()).ToArray());
}

public sealed class CreateOrderItemRequest
{
    [NotEmptyGuid]
    public Guid ProductId { get; init; }

    [Range(1, 10000)]
    public int Quantity { get; init; }

    public CreateOrderItemDto ToApplicationDto() => new(ProductId, Quantity);
}
