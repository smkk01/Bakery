using Bakery.Application.Repositories;
using Bakery.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Bakery.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string dataDirectoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dataDirectoryPath);

        services.Configure<JsonDatabaseOptions>(options => options.DirectoryPath = dataDirectoryPath);
        services.AddSingleton<IJsonDatabaseService, JsonDatabaseService>();
        services.AddSingleton<IProductRepository, JsonProductRepository>();
        services.AddSingleton<ICategoryRepository, JsonCategoryRepository>();
        services.AddSingleton<IOrderRepository, JsonOrderRepository>();
        services.AddSingleton<ICustomerRepository, JsonCustomerRepository>();
        services.AddSingleton<ICartRepository, JsonCartRepository>();
        return services;
    }
}
