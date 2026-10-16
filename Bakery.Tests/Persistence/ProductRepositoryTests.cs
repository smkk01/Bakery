using Bakery.Domain.Entities;
using Bakery.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bakery.Tests.Persistence;

public class ProductRepositoryTests
{
    private readonly JsonProductRepository _repository;
    private readonly string _databaseFolder;

    public ProductRepositoryTests()
    {
        _databaseFolder = Path.Combine(
            AppContext.BaseDirectory,
            "TestDatabase");

        if (Directory.Exists(_databaseFolder))
        {
            Directory.Delete(_databaseFolder, true);
        }

        Directory.CreateDirectory(_databaseFolder);

        var options = Options.Create(new JsonDatabaseOptions
        {
            DirectoryPath = _databaseFolder
        });

        var databaseService = new JsonDatabaseService(
            options,
            NullLogger<JsonDatabaseService>.Instance);

        _repository = new JsonProductRepository(databaseService);
    }

    [Fact]
    public async Task CreateAsync_ShouldPersistProduct()
    {
        // Arrange
        var product = new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Chocolate Cake",
            Description = "Rich chocolate cake",
            Price = 15.99m,
            ImageUrl = "/images/cake.jpg",
            IsAvailable = true
        };

        // Act
        await _repository.CreateAsync(product);

        var products = await _repository.GetAllAsync();

        // Assert
        products.Should().ContainSingle();

        products.First().Name.Should()
            .Be("Chocolate Cake");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnProducts()
    {
        // Arrange

        await _repository.CreateAsync(new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Cake A",
            Description = "Description for Cake A",
            Price = 20,
            ImageUrl = "/images/cake_a.jpg",
            IsAvailable = true
        });

        await _repository.CreateAsync(new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Cake B",
            Description = "Description for Cake B",
            Price = 30,
            ImageUrl = "/images/cake_b.jpg",
            IsAvailable = true
        });

        // Act

        var result = await _repository.GetAllAsync();

        // Assert

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProduct()
    {
        // Arrange

        var product = new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Delete Me",
            Description = "Description for Delete Me",
            Price = 10,
            ImageUrl = "/images/delete_me.jpg",
            IsAvailable = true
        };

        await _repository.CreateAsync(product);

        // Act

        var deleted =
            await _repository.DeleteAsync(product.Id);

        var products =
            await _repository.GetAllAsync();

        // Assert

        deleted.Should().BeTrue();

        products.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ShouldWriteToProductsJson()
    {
        // Arrange

        var product = new Product
        {
            Id = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            Name = "Json Test Cake",
            Description = "Description for Json Test Cake",
            Price = 20,
            ImageUrl = "/images/json_test_cake.jpg",
            IsAvailable = true
        };

        // Act

        await _repository.CreateAsync(product);

        var jsonPath = Path.Combine(
            _databaseFolder,
            "products.json");

        var json =
            await File.ReadAllTextAsync(jsonPath);

        // Assert

        json.Should()
            .Contain("Json Test Cake");
    }
}
