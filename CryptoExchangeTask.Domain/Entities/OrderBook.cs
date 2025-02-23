namespace CryptoExchangeTask.Domain.Entities;

/// <summary>
/// Represents the order book in the crypto exchange.
/// </summary>
public class OrderBook
{
    /// <summary>
    /// Gets the list of buy orders (bids).
    /// </summary>
    public IReadOnlyCollection<Bid> Bids { get; set; } = [];

    /// <summary>
    /// Gets the list of sell orders (asks).
    /// </summary>
    public IReadOnlyCollection<Ask> Asks { get; set; } = [];
}

/// <summary>
/// Represents a bid in the order book.
/// </summary>
public class Bid
{
    /// <summary>
    /// Gets or sets the order associated with the bid.
    /// </summary>
    public Order Order { get; set; } = new();
}

/// <summary>
/// Represents an ask in the order book.
/// </summary>
public class Ask
{
    /// <summary>
    /// Gets or sets the order associated with the ask.
    /// </summary>
    public Order Order { get; set; } = new();
}
