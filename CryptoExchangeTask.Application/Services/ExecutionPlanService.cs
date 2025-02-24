using CryptoExchangeTask.Application.Abstractions;
using CryptoExchangeTask.Application.DTOs;
using CryptoExchangeTask.Domain.Abstractions;

namespace CryptoExchangeTask.Application.Services;

public class ExecutionPlanService : IExecutionPlanService
{
    private readonly IExecutionPlanner _executionPlanner;

    private readonly IExchangeRepository _exchangeRepository;

    public ExecutionPlanService(IExecutionPlanner executionPlanner, IExchangeRepository exchangeRepository)
    {
        _executionPlanner = executionPlanner;
        _exchangeRepository = exchangeRepository;
    }

    public ExecutionPlanDto GetExecutionPlan(GetExecutionPlanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        // Validate input parameters
        if (request.OrderAmount <= 0)
        {
            throw new ArgumentException("Order amount must be greater than zero.", nameof(request.OrderAmount));
        }

        var orderAmount = request.OrderAmount;
        var orderType = request.OrderType;

        // Get all exchanges
        var exchanges = _exchangeRepository.GetAllExchanges().ToList();
        if (exchanges.Count == 0)
        {
            throw new InvalidOperationException("No exchanges found.");
        }

        var executionPlan = _executionPlanner.CalculateExecutionPlan(orderType, orderAmount, exchanges);

        // Map ExecutionPlan to ExecutionPlanDto
        var executionPlanResponse = new ExecutionPlanDto
        {
            TotalCost = executionPlan.TotalCost,
            Orders = executionPlan.Orders.Select(executionOrder => new ExecutionOrderDto
            {
                ExchangeId = executionOrder.ExchangeId,
                OrderId = executionOrder.OrderId,
                Price = executionOrder.Price,
                Amount = executionOrder.Amount
            }).ToList()
        };

        return executionPlanResponse;
    }
}
