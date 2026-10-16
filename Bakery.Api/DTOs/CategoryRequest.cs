using System.ComponentModel.DataAnnotations;
using Bakery.Domain.Entities;

namespace Bakery.Api.DTOs;

public sealed class CategoryRequest
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(1000, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;

    public Category ToEntity(Guid? id = null, DateTime? createdAtUtc = null)
    {
        var now = DateTime.UtcNow;
        return new Category
        {
            Id = id ?? Guid.NewGuid(),
            Name = Name.Trim(),
            Description = Description.Trim(),
            CreatedAtUtc = createdAtUtc ?? now,
            UpdatedAtUtc = now
        };
    }
}
