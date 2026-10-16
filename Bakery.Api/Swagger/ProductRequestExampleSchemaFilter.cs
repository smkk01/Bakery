using Bakery.Api.DTOs;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bakery.Api.Swagger;

public sealed class ProductRequestExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CategoryRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["name"] = new OpenApiString("Breads"),
                ["description"] = new OpenApiString("Freshly baked breads and rolls")
            };
            return;
        }

        if (context.Type == typeof(CartItemRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["customerId"] = new OpenApiString("22222222-2222-2222-2222-222222222222"),
                ["productId"] = new OpenApiString("33333333-3333-3333-3333-333333333333"),
                ["quantity"] = new OpenApiInteger(2)
            };
            return;
        }

        if (context.Type == typeof(RemoveCartItemRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["customerId"] = new OpenApiString("22222222-2222-2222-2222-222222222222"),
                ["productId"] = new OpenApiString("33333333-3333-3333-3333-333333333333")
            };
            return;
        }

        if (context.Type == typeof(CreateOrderRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["customerId"] = new OpenApiString("22222222-2222-2222-2222-222222222222"),
                ["items"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["productId"] = new OpenApiString("33333333-3333-3333-3333-333333333333"),
                        ["quantity"] = new OpenApiInteger(2)
                    }
                }
            };
            return;
        }

        if (context.Type == typeof(RegisterRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["fullName"] = new OpenApiString("Alex Baker"),
                ["email"] = new OpenApiString("alex@example.com"),
                ["phone"] = new OpenApiString("+15551234567"),
                ["password"] = new OpenApiString("BakerPass2026!")
            };
            return;
        }

        if (context.Type == typeof(LoginRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["email"] = new OpenApiString("alex@example.com"),
                ["password"] = new OpenApiString("BakerPass2026!")
            };
            return;
        }

        if (context.Type != typeof(ProductRequest))
        {
            return;
        }

        schema.Example = new OpenApiObject
        {
            ["categoryId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
            ["name"] = new OpenApiString("Sourdough loaf"),
            ["description"] = new OpenApiString("Slow-fermented country sourdough"),
            ["price"] = new OpenApiDouble(8.50),
            ["imageUrl"] = new OpenApiString("https://example.com/sourdough.jpg"),
            ["isAvailable"] = new OpenApiBoolean(true)
        };
    }
}
