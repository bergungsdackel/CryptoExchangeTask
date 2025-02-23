namespace CryptoExchangeTask.Application.DTOs;

/// <summary>
/// Represents an execution plan in the crypto exchange.
/// </summary>
public class ExecutionPlanDto
{
    /// <summary>
    /// Gets or sets the total cost of the execution plan.
    /// </summary>
    public decimal TotalCost { get; init; } = 0;

    /// <summary>
    /// Gets or sets the collection of execution orders.
    /// </summary>
    public ICollection<ExecutionOrderDto> Orders { get; init; } = [];
}
