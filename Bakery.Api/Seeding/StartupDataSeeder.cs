using Bakery.Application.Repositories;
using Bakery.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Bakery.Api.Seeding;

public sealed class StartupDataSeeder(
    ICategoryRepository categories,
    IProductRepository products,
    ILogger<StartupDataSeeder> logger)
{
    private static readonly SeedCategory[] SeedCategories =
    [
        new(
            Guid.Parse("10000000-0000-4000-8000-000000000001"),
            "Cakes",
            "Celebration cakes and everyday slices.",
            [
                new(Guid.Parse("20000000-0000-4000-8000-000000000001"), "Chocolate Fudge Cake", "Rich chocolate cake layered with smooth fudge frosting.", 28.00m),
                new(Guid.Parse("20000000-0000-4000-8000-000000000002"), "Vanilla Berry Cake", "Soft vanilla sponge layered with seasonal berries and cream.", 26.50m),
                new(Guid.Parse("20000000-0000-4000-8000-000000000003"), "Carrot Walnut Cake", "Spiced carrot cake with toasted walnuts and cream cheese frosting.", 24.00m)
            ]),
        new(
            Guid.Parse("10000000-0000-4000-8000-000000000002"),
            "Cupcakes",
            "Hand-frosted cupcakes baked in small batches.",
            [
                new(Guid.Parse("20000000-0000-4000-8000-000000000004"), "Red Velvet Cupcake", "Velvety cocoa cupcake finished with cream cheese frosting.", 4.25m),
                new(Guid.Parse("20000000-0000-4000-8000-000000000005"), "Lemon Meringue Cupcake", "Bright lemon cupcake topped with toasted meringue.", 4.00m),
                new(Guid.Parse("20000000-0000-4000-8000-000000000006"), "Salted Caramel Cupcake", "Vanilla cupcake with salted caramel buttercream.", 4.50m)
            ]),
        new(
            Guid.Parse("10000000-0000-4000-8000-000000000003"),
            "Cookies",
            "Freshly baked cookies with crisp edges and soft centers.",
            [
                new(Guid.Parse("20000000-0000-4000-8000-000000000007"), "Chocolate Chunk Cookie", "Golden cookie packed with dark chocolate chunks.", 2.75m),
                new(Guid.Parse("20000000-0000-4000-8000-000000000008"), "Oatmeal Raisin Cookie", "Chewy oat cookie with plump raisins and a hint of cinnamon.", 2.50m),
                new(Guid.Parse("20000000-0000-4000-8000-000000000009"), "Double Cocoa Cookie", "Deep cocoa cookie with chocolate chips throughout.", 2.75m)
            ]),
        new(
            Guid.Parse("10000000-0000-4000-8000-000000000004"),
            "Bread",
            "Slow-fermented loaves and savory bakery breads.",
            [
                new(Guid.Parse("20000000-0000-4000-8000-000000000010"), "Country Sourdough", "Naturally leavened loaf with a crackly crust and open crumb.", 8.50m),
                new(Guid.Parse("20000000-0000-4000-8000-000000000011"), "Rosemary Focaccia", "Olive-oil focaccia finished with rosemary and flaky salt.", 7.00m),
                new(Guid.Parse("20000000-0000-4000-8000-000000000012"), "Honey Oat Loaf", "Tender sandwich loaf sweetened with local honey and rolled oats.", 6.50m)
            ]),
        new(
            Guid.Parse("10000000-0000-4000-8000-000000000005"),
            "Pastries",
            "Buttery, flaky pastries baked fresh each morning.",
            [
                new(Guid.Parse("20000000-0000-4000-8000-000000000013"), "Butter Croissant", "Classic laminated croissant with a delicate honeycomb crumb.", 3.75m),
                new(Guid.Parse("20000000-0000-4000-8000-000000000014"), "Almond Pain au Chocolat", "Flaky pastry filled with chocolate and toasted almond cream.", 4.75m),
                new(Guid.Parse("20000000-0000-4000-8000-000000000015"), "Apple Turnover", "Golden puff pastry filled with cinnamon-spiced apples.", 4.25m)
            ])
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Startup catalog seeding started");

        var existingCategories = await categories.GetAllAsync(cancellationToken);
        if (existingCategories.Count > 0)
        {
            logger.LogInformation(
                "Category seeding skipped: found {CategoryCount} existing categories",
                existingCategories.Count);
        }
        else
        {
            logger.LogInformation("No categories found; seeding {CategoryCount} predefined categories", SeedCategories.Length);
            foreach (var seed in SeedCategories)
            {
                var existing = await categories.GetByIdAsync(seed.Id, cancellationToken);
                if (existing is not null)
                {
                    logger.LogInformation(
                        "Skipped category {CategoryName} ({CategoryId}): record already exists",
                        seed.Name,
                        seed.Id);
                    continue;
                }

                var category = new Category
                {
                    Id = seed.Id,
                    Name = seed.Name,
                    Description = seed.Description
                };
                await categories.CreateAsync(category, cancellationToken);
                logger.LogInformation(
                    "Seeded category {CategoryName} ({CategoryId})",
                    category.Name,
                    category.Id);
            }

            existingCategories = await categories.GetAllAsync(cancellationToken);
            logger.LogInformation(
                "Category seeding complete; repository now contains {CategoryCount} categories",
                existingCategories.Count);
        }

        var existingProducts = await products.GetAllAsync(cancellationToken);
        if (existingProducts.Count > 0)
        {
            logger.LogInformation(
                "Product seeding skipped: found {ProductCount} existing products",
                existingProducts.Count);
            logger.LogInformation("Startup catalog seeding completed without product changes");
            return;
        }

        logger.LogInformation("No products found; resolving categories before product seeding");
        var seededProductCount = 0;
        var skippedProductCount = 0;

        foreach (var categorySeed in SeedCategories)
        {
            var matchingCategory = existingCategories.FirstOrDefault(category => category.Id == categorySeed.Id);
            if (matchingCategory is null)
            {
                matchingCategory = existingCategories.FirstOrDefault(category =>
                    string.Equals(
                        NormalizeName(category.Name),
                        NormalizeName(categorySeed.Name),
                        StringComparison.OrdinalIgnoreCase));
            }

            if (matchingCategory is null)
            {
                skippedProductCount += categorySeed.Products.Length;
                logger.LogWarning(
                    "Skipped {ProductCount} products for category {CategoryName}: category seeding was skipped because categories already existed, but no matching category was found",
                    categorySeed.Products.Length,
                    categorySeed.Name);
                continue;
            }

            foreach (var productSeed in categorySeed.Products)
            {
                var existing = await products.GetByIdAsync(productSeed.Id, cancellationToken);
                if (existing is not null)
                {
                    logger.LogInformation(
                        "Skipped product {ProductName} ({ProductId}): record already exists",
                        productSeed.Name,
                        productSeed.Id);
                    continue;
                }

                var product = new Product
                {
                    Id = productSeed.Id,
                    CategoryId = matchingCategory.Id,
                    Name = productSeed.Name,
                    Description = productSeed.Description,
                    Price = productSeed.Price,
                    IsAvailable = true
                };
                await products.CreateAsync(product, cancellationToken);
                seededProductCount++;
                logger.LogInformation(
                    "Seeded product {ProductName} ({ProductId}) in category {CategoryName} ({CategoryId}) at {ProductPrice}",
                    product.Name,
                    product.Id,
                    matchingCategory.Name,
                    matchingCategory.Id,
                    product.Price);
            }
        }

        logger.LogInformation(
            "Product seeding complete; inserted {SeededProductCount} products and skipped {SkippedProductCount} products",
            seededProductCount,
            skippedProductCount);
        logger.LogInformation("Startup catalog seeding completed");
    }

    private static string NormalizeName(string name) => name.Trim();

    private sealed record SeedCategory(
        Guid Id,
        string Name,
        string Description,
        SeedProduct[] Products);

    private sealed record SeedProduct(Guid Id, string Name, string Description, decimal Price);
}
