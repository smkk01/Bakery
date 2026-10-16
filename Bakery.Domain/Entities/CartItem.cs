using System.ComponentModel.DataAnnotations;
using Bakery.Domain.Validation;

namespace Bakery.Domain.Entities;

public sealed class CartItem
{
    [NotEmptyGuid]
    public Guid ProductId { get; set; }

    [Required, StringLength(120, MinimumLength = 1)]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, 10000)]
    public int Quantity { get; set; }

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal UnitPrice { get; set; }

    public decimal Subtotal => Quantity * UnitPrice;
}
