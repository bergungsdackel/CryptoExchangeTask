namespace CryptoExchangeTask.Application.DTOs;

/// <summary>
/// Represents a request to get an execution plan.
/// </summary>
public class GetExecutionPlanRequest
{
    /// <summary>
    /// Gets or sets the type of the order.
    /// </summary>
    public Domain.Enums.OrderType OrderType { get; set; }

    /// <summary>
    /// Gets or sets the amount of the order.
    /// </summary>
    public decimal OrderAmount { get; set; }
}
