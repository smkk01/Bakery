using Bakery.Api.Seeding;
using Bakery.Application.Repositories;
using Bakery.Domain.Entities;
using Bakery.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bakery.Tests.Seeding;

public sealed class StartupDataSeederTests
{
    [Fact]
    public async Task SeedAsync_WhenCatalogIsEmpty_SeedsFiveCategoriesAndFifteenProducts()
    {
        var databaseDirectory = CreateDatabaseDirectory();
        try
        {
            var database = CreateDatabase(databaseDirectory);
            var categoryRepository = new JsonCategoryRepository(database);
            var productRepository = new JsonProductRepository(database);
            var seeder = new StartupDataSeeder(
                categoryRepository,
                productRepository,
                NullLogger<StartupDataSeeder>.Instance);

            await seeder.SeedAsync();

            var categories = await categoryRepository.GetAllAsync();
            var products = await productRepository.GetAllAsync();
            Assert.Equal(5, categories.Count);
            Assert.Equal(15, products.Count);
            Assert.Equal(
                new[] { "Bread", "Cakes", "Cookies", "Cupcakes", "Pastries" },
                categories.Select(category => category.Name).Order(StringComparer.Ordinal));
            Assert.All(products, product => Assert.Contains(categories, category => category.Id == product.CategoryId));
            Assert.All(categories, category => Assert.Equal(3, products.Count(product => product.CategoryId == category.Id)));
        }
        finally
        {
            Directory.Delete(databaseDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SeedAsync_WhenRunTwice_DoesNotCreateDuplicateRecords()
    {
        var databaseDirectory = CreateDatabaseDirectory();
        try
        {
            var database = CreateDatabase(databaseDirectory);
            var seeder = new StartupDataSeeder(
                new JsonCategoryRepository(database),
                new JsonProductRepository(database),
                NullLogger<StartupDataSeeder>.Instance);

            await seeder.SeedAsync();
            await seeder.SeedAsync();

            var categories = await database.ReadAsync<List<Category>>("categories.json");
            var products = await database.ReadAsync<List<Product>>("products.json");
            Assert.Equal(5, categories?.Count);
            Assert.Equal(15, products?.Count);
        }
        finally
        {
            Directory.Delete(databaseDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SeedAsync_WhenCategoriesExist_AddsMissingCategoriesAndProducts()
    {
        var databaseDirectory = CreateDatabaseDirectory();
        try
        {
            var database = CreateDatabase(databaseDirectory);
            var existingCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = " Cakes ",
                Description = "Existing category"
            };
            await database.WriteAsync("categories.json", new List<Category> { existingCategory });
            var categoryRepository = new JsonCategoryRepository(database);
            var productRepository = new JsonProductRepository(database);
            var seeder = new StartupDataSeeder(
                categoryRepository,
                productRepository,
                NullLogger<StartupDataSeeder>.Instance);

            await seeder.SeedAsync();

            var categories = await categoryRepository.GetAllAsync();
            var products = await productRepository.GetAllAsync();
            Assert.Equal(5, categories.Count);
            Assert.Equal(15, products.Count);
            Assert.Contains(categories, category => category.Id == existingCategory.Id);
            Assert.All(products, product => Assert.Contains(categories, category => category.Id == product.CategoryId));
            Assert.Equal(3, products.Count(product => product.CategoryId == existingCategory.Id));
        }
        finally
        {
            Directory.Delete(databaseDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SeedAsync_WhenProductsExist_PreservesExistingProductAndAddsMissingSeedProducts()
    {
        var databaseDirectory = CreateDatabaseDirectory();
        try
        {
            var database = CreateDatabase(databaseDirectory);
            var existingProduct = new Product
            {
                CategoryId = Guid.NewGuid(),
                Name = "Existing product",
                Description = "Must not be replaced",
                Price = 2m
            };
            await database.WriteAsync("products.json", new List<Product> { existingProduct });
            var categoryRepository = new JsonCategoryRepository(database);
            var productRepository = new JsonProductRepository(database);
            var seeder = new StartupDataSeeder(
                categoryRepository,
                productRepository,
                NullLogger<StartupDataSeeder>.Instance);

            await seeder.SeedAsync();

            Assert.Equal(5, (await categoryRepository.GetAllAsync()).Count);
            var products = await productRepository.GetAllAsync();
            Assert.Equal(16, products.Count);
            Assert.Contains(products, product =>
                product.Id == existingProduct.Id &&
                product.Name == existingProduct.Name &&
                product.Description == existingProduct.Description);
            Assert.Equal(15, products.Count(product => product.Id != existingProduct.Id));
            Assert.All(
                products.Where(product => product.Id != existingProduct.Id),
                product => Assert.False(string.IsNullOrWhiteSpace(product.ImageUrl)));
        }
        finally
        {
            Directory.Delete(databaseDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task SeedAsync_WhenPluralBreadCategoryExists_UsesItWithoutAddingDuplicateCategory()
    {
        var databaseDirectory = CreateDatabaseDirectory();
        try
        {
            var database = CreateDatabase(databaseDirectory);
            var breadCategory = new Category
            {
                Id = Guid.NewGuid(),
                Name = "Breads",
                Description = "Existing bread category"
            };
            await database.WriteAsync("categories.json", new List<Category> { breadCategory });
            var categoryRepository = new JsonCategoryRepository(database);
            var productRepository = new JsonProductRepository(database);
            var seeder = new StartupDataSeeder(
                categoryRepository,
                productRepository,
                NullLogger<StartupDataSeeder>.Instance);

            await seeder.SeedAsync();

            var categories = await categoryRepository.GetAllAsync();
            var products = await productRepository.GetAllAsync();
            Assert.Equal(5, categories.Count);
            Assert.Contains(categories, category => category.Id == breadCategory.Id);
            Assert.Equal(3, products.Count(product => product.CategoryId == breadCategory.Id));
        }
        finally
        {
            Directory.Delete(databaseDirectory, recursive: true);
        }
    }

    private static string CreateDatabaseDirectory()
    {
        var directory = Path.Combine(Path.GetTempPath(), "BakerySeederTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }

    private static JsonDatabaseService CreateDatabase(string directory) =>
        new(
            Options.Create(new JsonDatabaseOptions { DirectoryPath = directory }),
            NullLogger<JsonDatabaseService>.Instance);
}
