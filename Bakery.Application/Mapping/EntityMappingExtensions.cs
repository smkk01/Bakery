using Bakery.Application.DTOs;
using Bakery.Application.Products;
using Bakery.Domain.Entities;

namespace Bakery.Application.Mapping;

public static class EntityMappingExtensions
{
    public static CategoryDto ToDto(this Category category) =>
        new(category.Id, category.Name, category.Description);

    public static ProductDto ToDto(this Product product) =>
        new(
            product.Id,
            product.CategoryId,
            product.Name,
            product.Description,
            product.Price,
            product.ImageUrl,
            product.IsAvailable,
            product.CreatedAtUtc,
            product.UpdatedAtUtc);

    public static CustomerDto ToDto(this Customer customer) =>
        new(customer.Id, customer.FullName, customer.Email, customer.Phone);

    public static OrderItemDto ToDto(this OrderItem item) =>
        new(item.ProductId, item.ProductName, item.Quantity, item.UnitPrice);

    public static OrderDto ToDto(this Order order) =>
        new(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            order.OrderDate,
            order.Status,
            order.TotalAmount,
            order.Items.Select(item => item.ToDto()).ToArray());

    public static Product ToEntity(
        this ProductInput input,
        Guid? id = null,
        DateTime? createdAtUtc = null,
        DateTime? updatedAtUtc = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            CategoryId = input.CategoryId,
            Name = input.Name.Trim(),
            Description = input.Description.Trim(),
            Price = input.Price,
            ImageUrl = string.IsNullOrWhiteSpace(input.ImageUrl) ? null : input.ImageUrl.Trim(),
            IsAvailable = input.IsAvailable,
            CreatedAtUtc = createdAtUtc ?? DateTime.UtcNow,
            UpdatedAtUtc = updatedAtUtc ?? DateTime.UtcNow
        };

    public static Category ToEntity(this CategoryDto dto) =>
        new() { Id = dto.Id, Name = dto.Name, Description = dto.Description };

    public static Customer ToEntity(this CustomerDto dto) =>
        new()
        {
            Id = dto.Id,
            FullName = dto.FullName,
            Email = dto.Email,
            Phone = dto.Phone
        };

    public static OrderItem ToEntity(this OrderItemDto dto) =>
        new()
        {
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice
        };

    public static Order ToEntity(this OrderDto dto) =>
        new()
        {
            Id = dto.Id,
            OrderNumber = dto.OrderNumber,
            CustomerId = dto.CustomerId,
            OrderDate = dto.OrderDate,
            Status = dto.Status,
            TotalAmount = dto.TotalAmount,
            Items = dto.Items.Select(item => item.ToEntity()).ToList()
        };
}
