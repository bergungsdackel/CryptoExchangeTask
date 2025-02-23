using CryptoExchangeTask.Domain.Entities;
using CryptoExchangeTask.Domain.Enums;

namespace CryptoExchangeTask.Domain.UnitTest.Helper;

/// <summary>
/// A simple Test Data Builder for creating Exchange instances.
/// </summary>
internal class ExchangeBuilder
{
    private readonly Exchange _exchange;

    private ExchangeBuilder(string id, decimal crypto, decimal euro)
    {
        _exchange = new Exchange
        {
            Id = id,
            AvailableFunds = new FundStorage
            {
                Crypto = crypto,
                Euro = euro
            },
            OrderBook = new OrderBook()
        };
    }

    internal static ExchangeBuilder Create(string id, decimal crypto, decimal euro) => new(id, crypto, euro);

    internal ExchangeBuilder WithAsks(IEnumerable<(decimal Price, decimal Amount)> asks)
    {
        var askList = new List<Ask>();

        foreach (var (price, amount) in asks)
        {
            askList.Add(new Ask
            {
                Order = new Order
                {
                    Id = Guid.NewGuid(),
                    Time = DateTime.UtcNow,
                    Type = OrderType.Sell,
                    Kind = OrderKind.Limit,
                    Amount = amount,
                    Price = price
                }
            });
        }

        _exchange.OrderBook.Asks = askList;

        return this;
    }

    internal ExchangeBuilder WithBids(IEnumerable<(decimal Price, decimal Amount)> bids)
    {
        var bidList = new List<Bid>();

        foreach (var (price, amount) in bids)
        {
            bidList.Add(new Bid
            {
                Order = new Order
                {
                    Id = Guid.NewGuid(),
                    Time = DateTime.UtcNow,
                    Type = OrderType.Buy,
                    Kind = OrderKind.Limit,
                    Amount = amount,
                    Price = price                        
                }
            });
        }

        _exchange.OrderBook.Bids = bidList;

        return this;
    }

    internal Exchange Build() => _exchange;
}
