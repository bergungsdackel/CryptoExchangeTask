namespace CryptoExchangeTask.Application.DTOs;

/// <summary>
/// Represents an execution order in the crypto exchange.
/// </summary>
public class ExecutionOrderDto
{
    /// <summary>
    /// Gets the unique identifier for the exchange of the order.
    /// </summary>
    public required string ExchangeId { get; init; }

    /// <summary>
    /// Gets the unique identifier for the order.
    /// </summary>
    public required Guid OrderId { get; init; }

    /// <summary>
    /// Gets the price of the order.
    /// </summary>
    public decimal Price { get; init; }

    /// <summary>
    /// Gets the amount in Cryptocurrency.
    /// </summary>
    public decimal Amount { get; init; }
}
