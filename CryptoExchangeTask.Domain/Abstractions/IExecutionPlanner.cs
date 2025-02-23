using CryptoExchangeTask.Domain.Entities;
using CryptoExchangeTask.Domain.Enums;

namespace CryptoExchangeTask.Domain.Abstractions;

/// <summary>
/// Defines the interface for an execution planner that calculates execution plans for orders.
/// </summary>
public interface IExecutionPlanner
{
    /// <summary>
    /// Calculates the execution plan for a given order type and amount across multiple exchanges.
    /// </summary>
    /// <param name="orderType">The type of the order.</param>
    /// <param name="orderAmount">The amount of the order.</param>
    /// <param name="exchanges">The collection of exchanges to consider for the execution plan.</param>
    /// <returns>An execution plan detailing how the order should be executed across the provided exchanges.</returns>
    ExecutionPlan CalculateExecutionPlan(OrderType orderType, decimal orderAmount, IEnumerable<Exchange> exchanges);
}
