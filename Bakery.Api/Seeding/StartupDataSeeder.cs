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
                new(Guid.Parse("20000000-0000-4000-8000-000000000001"), "Chocolate Fudge Cake", "Rich chocolate cake layered with smooth fudge frosting.", 28.00m, "https://images.unsplash.com/photo-1578985545062-69928b1d9587"),
                new(Guid.Parse("20000000-0000-4000-8000-000000000002"), "Vanilla Berry Cake", "Soft vanilla sponge layered with seasonal berries and cream.", 26.50m, "https://images.unsplash.com/photo-1565958011703-44f9829ba187"),
                new(Guid.Parse("20000000-0000-4000-8000-000000000003"), "Carrot Walnut Cake", "Spiced carrot cake with toasted walnuts and cream cheese frosting.", 24.00m, "https://images.unsplash.com/photo-1606313564200-e75d5e30476c")
            ]),
        new(
            Guid.Parse("10000000-0000-4000-8000-000000000002"),
            "Cupcakes",
            "Hand-frosted cupcakes baked in small batches.",
            [
                new(Guid.Parse("20000000-0000-4000-8000-000000000004"), "Red Velvet Cupcake", "Velvety cocoa cupcake finished with cream cheese frosting.", 4.25m, "https://images.unsplash.com/photo-1563729784474-d77dbb933a9e"),
                new(Guid.Parse("20000000-0000-4000-8000-000000000005"), "Lemon Meringue Cupcake", "Bright lemon cupcake topped with toasted meringue.", 4.00m, "https://images.unsplash.com/photo-1488477181946-6428a0291777"),
                new(Guid.Parse("20000000-0000-4000-8000-000000000006"), "Salted Caramel Cupcake", "Vanilla cupcake with salted caramel buttercream.", 4.50m, "https://images.unsplash.com/photo-1619531040576-f9416740661b")
            ]),
        new(
            Guid.Parse("10000000-0000-4000-8000-000000000003"),
            "Cookies",
            "Freshly baked cookies with crisp edges and soft centers.",
            [
                new(Guid.Parse("20000000-0000-4000-8000-000000000007"), "Chocolate Chunk Cookie", "Golden cookie packed with dark chocolate chunks.", 2.75m, "https://images.unsplash.com/photo-1558961363-fa8fdf82db35"),
                new(Guid.Parse("20000000-0000-4000-8000-000000000008"), "Oatmeal Raisin Cookie", "Chewy oat cookie with plump raisins and a hint of cinnamon.", 2.50m, "https://images.unsplash.com/photo-1499636136210-6f4ee915583e"),
                new(Guid.Parse("20000000-0000-4000-8000-000000000009"), "Double Cocoa Cookie", "Deep cocoa cookie with chocolate chips throughout.", 2.75m, "https://images.unsplash.com/photo-1606313564200-e75d5e30476c")
            ]),
        new(
            Guid.Parse("10000000-0000-4000-8000-000000000004"),
            "Bread",
            "Slow-fermented loaves and savory bakery breads.",
            [
                new(Guid.Parse("20000000-0000-4000-8000-000000000010"), "Country Sourdough", "Naturally leavened loaf with a crackly crust and open crumb.", 8.50m, "https://images.unsplash.com/photo-1509440159596-0249088772ff"),
                new(Guid.Parse("20000000-0000-4000-8000-000000000011"), "Rosemary Focaccia", "Olive-oil focaccia finished with rosemary and flaky salt.", 7.00m, "https://images.unsplash.com/photo-1549931319-a545dcf3bc73"),
                new(Guid.Parse("20000000-0000-4000-8000-000000000012"), "Honey Oat Loaf", "Tender sandwich loaf sweetened with local honey and rolled oats.", 6.50m, "https://images.unsplash.com/photo-1586444248902-2f64eddc13df")
            ]),
        new(
            Guid.Parse("10000000-0000-4000-8000-000000000005"),
            "Pastries",
            "Buttery, flaky pastries baked fresh each morning.",
            [
                new(Guid.Parse("20000000-0000-4000-8000-000000000013"), "Butter Croissant", "Classic laminated croissant with a delicate honeycomb crumb.", 3.75m, "https://images.unsplash.com/photo-1623334044303-241021148842"),
                new(Guid.Parse("20000000-0000-4000-8000-000000000014"), "Almond Pain au Chocolat", "Flaky pastry filled with chocolate and toasted almond cream.", 4.75m, "https://images.unsplash.com/photo-1562376552-0d160a2f238d"),
                new(Guid.Parse("20000000-0000-4000-8000-000000000015"), "Apple Turnover", "Golden puff pastry filled with cinnamon-spiced apples.", 4.25m, "https://images.unsplash.com/photo-1530610476181-d83430b64dcd")
            ])
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Startup catalog seeding started");

        var existingCategories = (await categories.GetAllAsync(cancellationToken)).ToList();
        var insertedCategoryCount = 0;
        foreach (var seed in SeedCategories)
        {
            var existing = FindCategory(existingCategories, seed);
            if (existing is not null)
            {
                logger.LogInformation(
                    "Matched existing category {CategoryName} ({CategoryId}) to seed category {SeedCategoryName}",
                    existing.Name,
                    existing.Id,
                    seed.Name);
                continue;
            }

            var category = await categories.GetByIdAsync(seed.Id, cancellationToken);
            if (category is null)
            {
                category = new Category
                {
                    Id = seed.Id,
                    Name = seed.Name,
                    Description = seed.Description
                };
                await categories.CreateAsync(category, cancellationToken);
                insertedCategoryCount++;
                logger.LogInformation(
                    "Seeded category {CategoryName} ({CategoryId})",
                    category.Name,
                    category.Id);
            }
            else
            {
                logger.LogInformation(
                    "Skipped category {CategoryName} ({CategoryId}): record already exists",
                    category.Name,
                    category.Id);
            }

            existingCategories.Add(category);
        }
        logger.LogInformation(
            "Category seeding complete; inserted {InsertedCategoryCount} categories and matched {ExistingCategoryCount} existing categories",
            insertedCategoryCount,
            SeedCategories.Length - insertedCategoryCount);

        var existingProducts = (await products.GetAllAsync(cancellationToken)).ToList();
        var seededProductCount = 0;
        var skippedProductCount = 0;

        foreach (var categorySeed in SeedCategories)
        {
            var matchingCategory = FindCategory(existingCategories, categorySeed)
                ?? throw new InvalidOperationException($"Seed category '{categorySeed.Name}' was not created or matched.");

            foreach (var productSeed in categorySeed.Products)
            {
                var existing = existingProducts.FirstOrDefault(product =>
                    product.Id == productSeed.Id ||
                    product.CategoryId == matchingCategory.Id &&
                    string.Equals(
                        NormalizeName(product.Name),
                        NormalizeName(productSeed.Name),
                        StringComparison.OrdinalIgnoreCase));
                if (existing is not null)
                {
                    logger.LogInformation(
                        "Skipped product {ProductName} ({ProductId}): matching product {ExistingProductName} ({ExistingProductId}) already exists",
                        productSeed.Name,
                        productSeed.Id,
                        existing.Name,
                        existing.Id);
                    continue;
                }

                var product = new Product
                {
                    Id = productSeed.Id,
                    CategoryId = matchingCategory.Id,
                    Name = productSeed.Name,
                    Description = productSeed.Description,
                    Price = productSeed.Price,
                    ImageUrl = productSeed.ImageUrl,
                    IsAvailable = true
                };
                await products.CreateAsync(product, cancellationToken);
                existingProducts.Add(product);
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

    private static Category? FindCategory(IEnumerable<Category> existingCategories, SeedCategory seed) =>
        existingCategories.FirstOrDefault(category => category.Id == seed.Id)
        ?? existingCategories.FirstOrDefault(category =>
            string.Equals(
                NormalizeCategoryName(category.Name),
                NormalizeCategoryName(seed.Name),
                StringComparison.OrdinalIgnoreCase));

    private static string NormalizeName(string name) => name.Trim();

    private static string NormalizeCategoryName(string name)
    {
        var normalized = NormalizeName(name);
        if (normalized.EndsWith("ies", StringComparison.OrdinalIgnoreCase))
        {
            return $"{normalized[..^3]}y";
        }

        return normalized.EndsWith("s", StringComparison.OrdinalIgnoreCase)
            ? normalized[..^1]
            : normalized;
    }

    private sealed record SeedCategory(
        Guid Id,
        string Name,
        string Description,
        SeedProduct[] Products);

    private sealed record SeedProduct(Guid Id, string Name, string Description, decimal Price, string ImageUrl);
}
