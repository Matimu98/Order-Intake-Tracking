using Microsoft.Extensions.DependencyInjection;
using OrderIntakeTracking.Application.Interfaces;
using OrderIntakeTracking.Application.Services;
using OrderIntakeTracking.Infrastructure.Repositories;

namespace OrderIntakeTracking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }
}
