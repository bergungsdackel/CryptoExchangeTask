using CryptoExchangeTask.Application.Abstractions;
using CryptoExchangeTask.Application.DTOs;
using CryptoExchangeTask.Domain.Abstractions;
using CryptoExchangeTask.Domain.Enums;

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

    public ExecutionPlanDto GetExecutionPlan(OrderType orderType, decimal orderAmount)
    {
        // Validate input parameters
        if (orderAmount <= 0)
        {
            throw new ArgumentException("Order amount must be greater than zero.", nameof(orderAmount));
        }

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
                Price = executionOrder.Price,
                Amount = executionOrder.Amount
            }).ToList()
        };

        return executionPlanResponse;
    }
}
