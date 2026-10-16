using Bakery.Application.DTOs;
using Bakery.Application.Mapping;
using Bakery.Application.Repositories;
using Bakery.Domain.Entities;

namespace Bakery.Application.Products;

public sealed class ProductService(IProductRepository repository) : IProductService
{
    public async Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await repository.GetAllAsync(cancellationToken)).Select(product => product.ToDto()).ToArray();

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await repository.GetByIdAsync(id, cancellationToken))?.ToDto();

    public async Task<ProductDto> CreateAsync(ProductInput input, CancellationToken cancellationToken = default)
    {
        Validate(input);
        var product = input.ToEntity();

        await repository.CreateAsync(product, cancellationToken);
        return product.ToDto();
    }

    public async Task<ProductDto?> UpdateAsync(
        Guid id,
        ProductInput input,
        CancellationToken cancellationToken = default)
    {
        Validate(input);
        var existing = await repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        var updated = input.ToEntity(existing.Id, existing.CreatedAtUtc);

        var saved = await repository.UpdateAsync(updated, cancellationToken);
        return saved ? updated.ToDto() : null;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        repository.DeleteAsync(id, cancellationToken);

    private static void Validate(ProductInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw new ArgumentException("A product name is required.", nameof(input));
        }

        if (input.CategoryId == Guid.Empty)
        {
            throw new ArgumentException("A category is required.", nameof(input));
        }

        if (string.IsNullOrWhiteSpace(input.Description))
        {
            throw new ArgumentException("A product description is required.", nameof(input));
        }

        if (input.Price <= 0)
        {
            throw new ArgumentException("Product price must be greater than zero.", nameof(input));
        }
    }

}
