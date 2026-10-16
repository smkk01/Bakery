using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Bakery.Domain.Validation;

namespace Bakery.Domain.Entities;

public sealed class Cart : BaseEntity
{
    [NotEmptyGuid]
    public Guid CustomerId { get; set; }

    [MinLength(0)]
    public List<CartItem> Items { get; set; } = [];

    [JsonIgnore]
    public decimal Subtotal => Items.Sum(item => item.Subtotal);

    [JsonIgnore]
    public decimal Total => Subtotal;
}
