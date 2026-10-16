using System.ComponentModel.DataAnnotations;
using Bakery.Domain.Enums;
using Bakery.Domain.Validation;

namespace Bakery.Domain.Entities;

public sealed class Order : BaseEntity
{
    [Required, StringLength(44, MinimumLength = 1)]
    public string OrderNumber { get; set; } = string.Empty;

    [NotEmptyGuid]
    public Guid CustomerId { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [EnumDataType(typeof(OrderStatus))]
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Range(typeof(decimal), "0", "100000000")]
    public decimal TotalAmount { get; set; }

    [MinLength(1)]
    public List<OrderItem> Items { get; set; } = [];
}
