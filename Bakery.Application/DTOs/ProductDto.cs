namespace Bakery.Application.DTOs;

public sealed record ProductDto(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    string? ImageUrl,
    bool IsAvailable,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
