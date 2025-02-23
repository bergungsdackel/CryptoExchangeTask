using CryptoExchangeTask.Domain.Abstractions;
using CryptoExchangeTask.Domain.Entities;
using CryptoExchangeTask.Domain.Enums;
using CryptoExchangeTask.Domain.Exceptions;

namespace CryptoExchangeTask.Domain.Services;

/// <summary>
/// Provides implementation for the execution planner that calculates execution plans for orders.
/// </summary>
public class ExecutionPlanner : IExecutionPlanner
{
    /// <inheritdoc />
    public ExecutionPlan CalculateExecutionPlan(OrderType orderType, decimal orderAmount, IEnumerable<Exchange> exchanges)
    {
        return orderType switch
        {
            OrderType.Buy => CalculateBuyExecutionPlan(orderAmount, exchanges),
            OrderType.Sell => CalculateSellExecutionPlan(orderAmount, exchanges),
            _ => throw new ArgumentOutOfRangeException(nameof(orderType), orderType, "Invalid order type."),
        };
    }

    private static ExecutionPlan CalculateBuyExecutionPlan(decimal orderAmount, IEnumerable<Exchange> exchanges)
    {
        // Build order candidates from the asks of each exchange and prepare available funds (in Euro)
        var candidateOrders = new List<OrderCandidate>();
        var availableFunds = exchanges.ToDictionary(exchange => exchange.Id, exchange => exchange.AvailableFunds.Euro); // Key: Exchange ID, Value: Available Euro funds

        foreach (var exchange in exchanges)
        {
            foreach (var ask in exchange.OrderBook.Asks)
            {
                candidateOrders.Add(new OrderCandidate
                {
                    ExchangeId = exchange.Id,
                    Price = ask.Order.Price,
                    Amount = ask.Order.Amount
                });
            }
        }

        // For buy orders, we want to buy the cheapest first
        candidateOrders = candidateOrders.OrderBy(candidateOrder => candidateOrder.Price).ToList();

        return ProcessCandidates(candidateOrders, orderAmount, OrderType.Buy, availableFunds);
    }

    private static ExecutionPlan CalculateSellExecutionPlan(decimal orderAmount, IEnumerable<Exchange> exchanges)
    {
        // Build order candidates from the bids of each exchange and prepare available funds (in Crypto)
        var candidateOrders = new List<OrderCandidate>();
        var availableFunds = exchanges.ToDictionary(exchange => exchange.Id, exchange => exchange.AvailableFunds.Crypto); // Key: Exchange ID, Value: Available Crypto funds

        foreach (var exchange in exchanges)
        {
            foreach (var bid in exchange.OrderBook.Bids)
            {
                candidateOrders.Add(new OrderCandidate
                {
                    ExchangeId = exchange.Id,
                    Price = bid.Order.Price,
                    Amount = bid.Order.Amount
                });
            }
        }

        // For sell orders, we want to sell the most expensive first
        candidateOrders = candidateOrders.OrderByDescending(candidateOrder => candidateOrder.Price).ToList();

        return ProcessCandidates(candidateOrders, orderAmount, OrderType.Sell, availableFunds);
    }

    private static ExecutionPlan ProcessCandidates(
        ICollection<OrderCandidate> candidateOrders,
        decimal orderAmount,
        OrderType orderType,
        Dictionary<string, decimal> availableFunds)
    {
        var executionPlan = new ExecutionPlan();
        var remainingAmount = orderAmount;

        foreach (var candidate in candidateOrders)
        {
            if (remainingAmount <= 0)
            {
                break;
            }

            decimal executableAmount = 0;

            if (orderType == OrderType.Buy)
            {
                // For buy orders, available funds are in Euro
                // Maximum Crypto that can be bought from this candidate is (remaining Euro funds / candidate.Price)
                decimal maxByFunds = availableFunds[candidate.ExchangeId] / candidate.Price;
                executableAmount = Math.Min(candidate.Amount, Math.Min(remainingAmount, maxByFunds));

                if (executableAmount > 0)
                {
                    executionPlan.AddOrder(new ExecutionOrder
                    {
                        ExchangeId = candidate.ExchangeId,
                        Price = candidate.Price,
                        Amount = executableAmount
                    });

                    // Deduct the cost in Euro from the funds of this particular exchange
                    availableFunds[candidate.ExchangeId] -= executableAmount * candidate.Price;
                }
            }
            else if (orderType == OrderType.Sell)
            {
                // For sell orders, available funds are in Crypto
                decimal maxByFunds = availableFunds[candidate.ExchangeId];
                executableAmount = Math.Min(candidate.Amount, Math.Min(remainingAmount, maxByFunds));

                if (executableAmount > 0)
                {
                    executionPlan.AddOrder(new ExecutionOrder
                    {
                        ExchangeId = candidate.ExchangeId,
                        Price = candidate.Price,
                        Amount = executableAmount
                    });

                    // Deduct the sold amount from the available Crypto of this particular exchange
                    availableFunds[candidate.ExchangeId] -= executableAmount;
                }
            }

            // Update the remaining amount to be processed
            remainingAmount -= executableAmount;
        }

        if (remainingAmount > 0)
        {
            throw new InsufficientLiquidityException();
        }

        return executionPlan;
    }

    // Helper class to store order candidates
    private class OrderCandidate
    {
        public required string ExchangeId { get; set; }

        public required decimal Price { get; set; }

        public required decimal Amount { get; set; }
    }
}
