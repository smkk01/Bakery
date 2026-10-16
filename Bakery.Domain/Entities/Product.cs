using System.ComponentModel.DataAnnotations;
using Bakery.Domain.Validation;

namespace Bakery.Domain.Entities;

public sealed class Product : BaseEntity
{
    [NotEmptyGuid]
    public Guid CategoryId { get; set; }

    [Required, StringLength(120, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Price { get; set; }

    [Url, StringLength(2048)]
    public string? ImageUrl { get; set; }

    public bool IsAvailable { get; set; } = true;
}
