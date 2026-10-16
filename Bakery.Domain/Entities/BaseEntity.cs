using System.ComponentModel.DataAnnotations;

namespace Bakery.Domain.Entities;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
