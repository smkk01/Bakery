using Bakery.Application.DTOs;

namespace Bakery.Application.Products;

public interface IProductService
{
    Task<IReadOnlyList<ProductDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ProductDto> CreateAsync(ProductInput input, CancellationToken cancellationToken = default);

    Task<ProductDto?> UpdateAsync(Guid id, ProductInput input, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
