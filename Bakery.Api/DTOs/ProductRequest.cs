using System.ComponentModel.DataAnnotations;
using Bakery.Application.Products;
using Bakery.Domain.Validation;

namespace Bakery.Api.DTOs;

public sealed class ProductRequest
{
    [NotEmptyGuid]
    public Guid CategoryId { get; init; }

    [Required, StringLength(120, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 1)]
    public string Description { get; init; } = string.Empty;

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Price { get; init; }

    [Url, StringLength(2048)]
    public string? ImageUrl { get; init; }

    public bool IsAvailable { get; init; } = true;

    public ProductInput ToInput() =>
        new(CategoryId, Name.Trim(), Description.Trim(), Price, NormalizeImageUrl(ImageUrl), IsAvailable);

    private static string? NormalizeImageUrl(string? imageUrl) =>
        string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
}
