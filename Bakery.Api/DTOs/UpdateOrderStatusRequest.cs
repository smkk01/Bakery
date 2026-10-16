using System.ComponentModel.DataAnnotations;
using Bakery.Domain.Enums;

namespace Bakery.Api.DTOs;

public sealed class UpdateOrderStatusRequest
{
    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; init; }
}
