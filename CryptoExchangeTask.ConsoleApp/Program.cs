using CryptoExchangeTask.Application;
using CryptoExchangeTask.Application.Abstractions;
using CryptoExchangeTask.Application.DTOs;
using CryptoExchangeTask.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CryptoExchangeTask.ConsoleApp;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Add services to the container
        builder.Services.AddApplicationServices(builder.Configuration);

        using var host = builder.Build();
        await RunAsync(host.Services).ConfigureAwait(false);
    }

    private static async Task RunAsync(IServiceProvider services)
    {
        await using var appScope = services.CreateAsyncScope();
        var serviceProvider = appScope.ServiceProvider;
        var executionService = serviceProvider.GetRequiredService<IExecutionPlanService>();

        // Get order type from user
        if(!TryGetOrderTypeFromUser(out OrderType orderType))
        {
            Console.WriteLine("Invalid order type. Exiting.");
            return;
        }

        // Get order amount from user
        if (!TryGetOrderAmountFromUser(out decimal orderAmount))
        {
            Console.WriteLine("Invalid amount. Exiting.");
            return;
        }

        try
        {
            // Get the execution plan
            var executionPlan = executionService.GetExecutionPlan(orderType, orderAmount);

            // Print execution plan
            PrintExecutionPlan(executionPlan, orderType);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing the order: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static bool TryGetOrderTypeFromUser(out OrderType orderType)
    {
        Console.WriteLine("Enter order type (buy/sell): ");

        var input = Console.ReadLine()?.Trim().ToLower();
        return Enum.TryParse(input, true, out orderType);
    }

    private static bool TryGetOrderAmountFromUser(out decimal orderAmount)
    {
        Console.WriteLine("Enter the amount of Crypto: ");

        var input = Console.ReadLine()?.Trim();
        return decimal.TryParse(input, out orderAmount);
    }

    private static void PrintExecutionPlan(ExecutionPlanDto executionPlan, OrderType orderType)
    {
        Console.WriteLine("Execution Plan:");

        foreach (var order in executionPlan.Orders)
        {
            Console.WriteLine($"Exchange: {order.ExchangeId}; Price: {order.Price}; Amount: {order.Amount}");
        }

        Console.WriteLine($"Total {(orderType == OrderType.Buy ? "Cost" : "Revenue")}: {executionPlan.TotalCost}");
    }
}
