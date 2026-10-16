using System.ComponentModel.DataAnnotations;

namespace Bakery.Domain.Entities;

public sealed class Category : BaseEntity
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;
}
