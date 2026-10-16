using Bakery.Domain.Validation;

namespace Bakery.Api.DTOs;

public sealed class RemoveCartItemRequest
{
    [NotEmptyGuid]
    public Guid CustomerId { get; init; }

    [NotEmptyGuid]
    public Guid ProductId { get; init; }
}
