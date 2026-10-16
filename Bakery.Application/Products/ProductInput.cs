namespace Bakery.Application.Products;

public sealed record ProductInput(
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable);
