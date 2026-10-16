using Bakery.Application.Cart;
using Bakery.Application.Authentication;
using Bakery.Application.Orders;
using Bakery.Application.Products;
using Microsoft.Extensions.DependencyInjection;

namespace Bakery.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<ICartService, CartService>();
        return services;
    }
}
