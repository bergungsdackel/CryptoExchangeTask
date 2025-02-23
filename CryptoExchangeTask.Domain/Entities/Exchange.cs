namespace CryptoExchangeTask.Domain.Entities;

/// <summary>
/// Represents a crypto exchange.
/// </summary>
public class Exchange
{
    /// <summary>
    /// Gets or sets the unique identifier for the exchange.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the available funds in the exchange.
    /// </summary>
    public FundStorage AvailableFunds { get; init; } = new();

    /// <summary>
    /// Gets or sets the order book of the exchange.
    /// </summary>
    public OrderBook OrderBook { get; init; } = new();
}