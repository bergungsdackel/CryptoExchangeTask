using CryptoExchangeTask.Application.Abstractions;
using CryptoExchangeTask.Application.Services;
using CryptoExchangeTask.Domain.Abstractions;
using CryptoExchangeTask.Domain.Services;
using CryptoExchangeTask.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CryptoExchangeTask.Application;

public static class Extensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Domain Services
        services.AddScoped<IExecutionPlanner, ExecutionPlanner>();

        // Application Services
        services.AddScoped<IExecutionPlanService, ExecutionPlanService>();

        // Infrastructure Services
        services.AddScoped<IExchangeRepository, JsonExchangeRepository>(provider =>
        {
            var jsonFileDirectory = configuration["ExchangeJsonDirectory"] ?? throw new ArgumentException("ExchangeJsonDirectory configuration is missing.");

            var fullJsonFileDirectoryPath = Path.Combine(AppContext.BaseDirectory, jsonFileDirectory);
            return new JsonExchangeRepository(fullJsonFileDirectoryPath, provider.GetRequiredService<ILogger<JsonExchangeRepository>>());
        });

        return services;
    }
}
