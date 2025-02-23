namespace CryptoExchangeTask.Domain.Entities;

/// <summary>
/// Represents a storage for funds available on an exchange.
/// <para>
/// <c>Crypto</c> holds the amount of Crypto available – used for sell orders (you can only sell what you own).
/// <c>Euro</c> holds the amount of Euro available – used for buy orders (you can only buy as much as your Euro can cover).
/// </para>
/// </summary>
public class FundStorage
{
    /// <summary>
    /// Gets or sets the amount of Cryptocurrency.
    /// This represents the BTC available for sell orders.
    /// </summary>
    public decimal Crypto { get; set; } = 0;

    /// <summary>
    /// Gets or sets the amount of Euro.
    /// This represents the Euro available for buy orders.
    /// </summary>
    public decimal Euro { get; set; } = 0;
}
