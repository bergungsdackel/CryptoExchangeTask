using CryptoExchangeTask.Application.DTOs;

namespace CryptoExchangeTask.Application.Abstractions;

/// <summary>
/// Provides methods to generate execution plans for orders across multiple exchanges.
/// </summary>
public interface IExecutionPlanService
{
    /// <summary>
    /// Gets the execution plan for a given order type and amount across multiple exchanges.
    /// </summary>
    /// <param name="request">The request containing order type and amount details.</param>
    /// <returns>An execution plan detailing how the order should be executed across exchanges.</returns>
    ExecutionPlanDto GetExecutionPlan(GetExecutionPlanRequest request);
}
