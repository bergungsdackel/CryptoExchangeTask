using CryptoExchangeTask.Domain.Enums;

namespace CryptoExchangeTask.Domain.Entities;

/// <summary>
/// Represents an order in the crypto exchange.
/// </summary>
public class Order
{
    /// <summary>
    /// Gets or sets the unique identifier for the order.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the time when the order was created.
    /// </summary>
    public DateTime Time { get; init; }

    /// <summary>
    /// Gets or sets the type of the order.
    /// </summary>
    public OrderType Type { get; init; }

    /// <summary>
    /// Gets or sets the kind of the order.
    /// </summary>
    public OrderKind Kind { get; init; }

    /// <summary>
    /// Gets or sets the amount of the order.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Gets or sets the price of the order.
    /// </summary>
    public decimal Price { get; init; }
}
