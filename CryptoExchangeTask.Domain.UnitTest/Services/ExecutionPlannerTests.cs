using CryptoExchangeTask.Domain.Entities;
using CryptoExchangeTask.Domain.Enums;
using CryptoExchangeTask.Domain.Exceptions;
using CryptoExchangeTask.Domain.Services;
using CryptoExchangeTask.Domain.UnitTest.Helper;
using Shouldly;

namespace CryptoExchangeTask.Domain.UnitTest.Services;

/// <summary>
/// This test class verifies that the <see cref="ExecutionPlanner"/> correctly computes execution plans under the following domain constraints:
/// - For buy orders: Available funds are in Euro and must be converted to Crypto using the order candidate ask price.
/// - For sell orders: Available funds are in Crypto and limit the sellable amount.
/// The tests ensure that candidate orders are processed in the correct order (cheapest for buys, highest for sells)
/// and that cumulative allocations from the same exchange do not exceed its available funds.
/// </summary>
public class ExecutionPlannerTests
{
    private ExecutionPlanner _planner;

    [SetUp]
    public void Setup()
    {
        _planner = new ExecutionPlanner();
    }

    [Test]
    [Description("Should select the cheapest asks first when executing a buy order across multiple exchanges.")]
    public void It_should_select_cheapest_asks_for_buy_orders()
    {
        // Arrange
        // For buy orders, available funds in Euro matter
        // Exchange1: available Euro = 10000, dummy Crypto value
        // Exchange2: available Euro = 20000, dummy Crypto value
        // Using ask orders with prices such that Euro conversion (Euro / price) does not limit the candidate
        var exchange1 = ExchangeBuilder.Create("ex1", crypto: 100, euro: 10000)
            .WithAsks(
            [
                (3000, 2), // Funds allow up to ~3.33 BTC at 3000, ask offers 2 BTC
                (3100, 2)
            ])
            .Build();

        var exchange2 = ExchangeBuilder.Create("ex2", crypto: 100, euro: 20000)
            .WithAsks(
            [
                    (2900, 5), // Funds allow up to ~6.90 BTC at 2900, ask offers 5 BTC
                    (3000, 3)
            ])
            .Build();

        decimal orderAmount = 6; // Total BTC to buy
        var exchanges = new List<Exchange> { exchange1, exchange2 };

        // Act
        var executionPlan = _planner.CalculateExecutionPlan(OrderType.Buy, orderAmount, exchanges);

        // Assert
        var executionPlanOrders = executionPlan.Orders.ToList();

        // Expect the cheapest ask from exchange2 (5 BTC at 2900) to be used first and then 1 BTC from exchange1 (at 3000) to complete the 6 BTC order
        executionPlanOrders.Count.ShouldBe(2);

        executionPlanOrders[0].ExchangeId.ShouldBe(exchange2.Id);
        executionPlanOrders[0].Price.ShouldBe(2900);
        executionPlanOrders[0].Amount.ShouldBe(5);

        executionPlanOrders[1].ExchangeId.ShouldBe(exchange1.Id);
        executionPlanOrders[1].Price.ShouldBe(3000);
        executionPlanOrders[1].Amount.ShouldBe(1);
    }

    [Test]
    [Description("Should limit buy order execution by available Euro funds converted to BTC.")]
    public void It_should_limit_buy_order_by_euro_conversion()
    {
        // Arrange
        // Available Euro funds are insufficient to buy the full order after conversion
        // E.g. available Euro = 5000 at an ask price of 3000 gives a max of ~1.67 BTC
        // Even though the ask offers 2 BTC, the exchange can only afford ~1.67 BTC
        var exchange = ExchangeBuilder.Create("ex1", crypto: 100, euro: 5000)
            .WithAsks(
            [
                (3000, 2) // Although ask offers 2 BTC, funds only allow ~1.67 BTC
            ])
            .Build();

        decimal orderAmount = 2; // Total BTC to buy
        var exchanges = new List<Exchange> { exchange };

        // Act & Assert
        // Should throw an InsufficientLiquidityException because available Euro funds only allow ~1.67 BTC
        Should.Throw<InsufficientLiquidityException>(() => _planner.CalculateExecutionPlan(OrderType.Buy, orderAmount, exchanges));
    }

    [Test]
    [Description("Should select the highest bids first when executing a sell order across multiple exchanges.")]
    public void It_should_select_highest_bids_for_sell_orders()
    {
        // Arrange
        // For sell orders, available funds in Crypto matter
        // Exchange1 and Exchange2 have sufficient Crypto in this case
        var exchange1 = ExchangeBuilder.Create("ex1", crypto: 100, euro: 100000)
            .WithBids(
            [
                (3100, 2) // Can sell 2 BTC
            ])
            .Build();

        var exchange2 = ExchangeBuilder.Create("ex2", crypto: 100, euro: 100000)
            .WithBids(
            [
                (3000, 3),
                (2900, 4)
            ])
            .Build();

        decimal orderAmount = 3; // Total BTC to sell
        var exchanges = new List<Exchange> { exchange1, exchange2 };

        // Act
        var executionPlan = _planner.CalculateExecutionPlan(OrderType.Sell, orderAmount, exchanges);

        // Assert
        var executionPlanOrders = executionPlan.Orders.ToList();

        // Expect the highest bid from exchange1 (2 BTC at 3100) used first, then 1 BTC from exchange2 (at 3000) to complete the 3 BTC sell order
        executionPlanOrders.Count.ShouldBe(2);

        executionPlanOrders[0].ExchangeId.ShouldBe(exchange1.Id);
        executionPlanOrders[0].Price.ShouldBe(3100);
        executionPlanOrders[0].Amount.ShouldBe(2);

        executionPlanOrders[1].ExchangeId.ShouldBe(exchange2.Id);
        executionPlanOrders[1].Price.ShouldBe(3000);
        executionPlanOrders[1].Amount.ShouldBe(1);
    }

    [Test]
    [Description("Should limit sell order execution by available Crypto funds.")]
    public void It_should_limit_sell_order_by_crypto_funds()
    {
        // Arrange
        // Available Crypto funds are insufficient relative to the bid order amount
        // For example, available Crypto = 1.5, but bid order offers 2 BTC
        var exchange = ExchangeBuilder.Create("ex1", crypto: 1.5m, euro: 100000)
            .WithBids(
            [
                (3100, 2)
            ])
            .Build();

        decimal orderAmount = 2; // Total BTC to sell
        var exchanges = new List<Exchange> { exchange };

        // Act & Assert
        // Should throw an InsufficientLiquidityException because available Crypto (1.5) is insufficient
        Should.Throw<InsufficientLiquidityException>(() => _planner.CalculateExecutionPlan(OrderType.Sell, orderAmount, exchanges));
    }

    [Test]
    [Description("Should throw an InsufficientLiquidityException when available Euro funds are insufficient for a buy order.")]
    public void It_should_throw_InsufficientLiquidityException_for_buy_orders_when_funds_are_insufficient()
    {
        // Arrange
        // Exchange where available Euro funds are insufficient:
        // Available Euro = 2000 at an ask price of 3000 gives a max of ~0.67 BTC
        var exchange = ExchangeBuilder.Create("ex1", crypto: 100, euro: 2000)
            .WithAsks(
            [
                (3000, 3) // Ask offers 3 BTC, but available Euro funds only allow ~0.67 BTC
            ])
            .Build();

        decimal orderAmount = 3; // Total BTC to buy
        var exchanges = new List<Exchange> { exchange };

        // Act & Assert
        // Should throw an InsufficientLiquidityException because available Euro funds only allow ~0.67 BTC
        Should.Throw<InsufficientLiquidityException>(() =>_planner.CalculateExecutionPlan(OrderType.Buy, orderAmount, exchanges));
    }

    [Test]
    [Description("Should throw an InsufficientLiquidityException when total liquidity is insufficient for a sell order.")]
    public void It_should_throw_InsufficientLiquidityException_for_sell_orders_when_liquidity_is_insufficient()
    {
        // Arrange
        // Exchange where the total available bid amount is less than the desired sell orders
        var exchange = ExchangeBuilder.Create("ex1", crypto: 100, euro: 100000)
            .WithBids(
            [
                (3000, 1)
            ])
            .Build();

        decimal orderAmount = 2;  // Total BTC to sell
        var exchanges = new List<Exchange> { exchange };

        // Act & Assert
        // Should throw an InsufficientLiquidityException because the total available bid amount is less than the desired sell order
        Should.Throw<InsufficientLiquidityException>(() => _planner.CalculateExecutionPlan(OrderType.Sell, orderAmount, exchanges));
    }

    [Test]
    [Description("Should not allow cumulative buy executionPlanOrders from the same exchange to exceed available Euro funds.")]
    public void It_should_not_allow_buy_orders_exceeding_available_euro_funds()
    {
        // Arrange
        // Single exchange with only 6000 EUR available
        // Two ask orders at the same price: Each order candidate would individually allow 6000/3000 = 2 BTC, but cumulatively they must not exceed the available funds
        var exchange = ExchangeBuilder.Create("ex1", crypto: 100, euro: 6000)
            .WithAsks(
            [
                (3000, 1.5m),
                (3000, 1.5m)
            ])
            .Build();

        decimal orderAmount = 2.5m; // Total BTC to buy; Request 2.5 BTC, but funds only allow 6000/3000 = 2 BTC
        var exchanges = new List<Exchange> { exchange };

        // Act & Assert
        // Should throw an InsufficientLiquidityException because the cumulative buy orders from the same exchange exceed available Euro funds
        Should.Throw<InsufficientLiquidityException>(() =>_planner.CalculateExecutionPlan(OrderType.Buy, orderAmount, exchanges));
    }

    [Test]
    [Description("Should not allow cumulative sell executionPlanOrders from the same exchange to exceed available Crypto funds.")]
    public void It_should_not_allow_sell_orders_exceeding_available_crypto_funds()
    {
        // Arrange
        // Single exchange with only 1 BTC available
        // Two bid orders with amounts that cumulatively exceed the available Crypto
        var exchange = ExchangeBuilder.Create("ex1", crypto: 1, euro: 100000)
            .WithBids(
            [
                (3100, 0.7m),
                (3000, 0.7m)
            ])
            .Build();

        decimal orderAmount = 1.2m; // Total BTC to sell; Request to sell 1.2 BTC, exceeding the available 1 BTC
        var exchanges = new List<Exchange> { exchange };

        // Act & Assert
        // Should throw an InsufficientLiquidityException because the cumulative sell orders from the same exchange exceed available Crypto funds
        Should.Throw<InsufficientLiquidityException>(() => _planner.CalculateExecutionPlan(OrderType.Sell, orderAmount, exchanges));
    }

    [Test]
    [Description("Boundary test: Should execute buy order exactly when available Euro funds exactly match order requirements.")]
    public void It_should_execute_buy_order_when_funds_exactly_match()
    {
        // Arrange
        // Available Euro = 3000 and ask price = 3000 gives exactly 1 BTC
        var exchange = ExchangeBuilder.Create("ex1", crypto: 100, euro: 3000)
            .WithAsks(
            [
                (3000, 1)
            ])
            .Build();

        decimal orderAmount = 1; // Total BTC to buy: Request exactly 1 BTC
        var exchanges = new List<Exchange> { exchange };

        // Act
        var executionPlan = _planner.CalculateExecutionPlan(OrderType.Buy, orderAmount, exchanges);

        // Assert
        var executionPlanOrders = executionPlan.Orders.ToList();

        // Expect a single order that exactly uses the available funds
        executionPlanOrders.Count.ShouldBe(1);

        executionPlanOrders[0].ExchangeId.ShouldBe(exchange.Id);
        executionPlanOrders[0].Price.ShouldBe(3000);
        executionPlanOrders[0].Amount.ShouldBe(1);
    }

    [Test]
    [Description("Boundary test: Should execute sell order exactly when available Crypto funds exactly match order requirements.")]
    public void It_should_execute_sell_order_when_funds_exactly_match()
    {
        // Arrange
        // Available Crypto = 1 BTC and a bid order that offers exactly 1 BTC at a specific price
        var exchange = ExchangeBuilder.Create("ex1", crypto: 1, euro: 100000)
            .WithBids(
            [
                (3100, 1) // Bid offering exactly 1 BTC at 3100
            ])
            .Build();

        decimal orderAmount = 1; // Total BTC to sell: Request exactly 1 BTC
        var exchanges = new List<Exchange> { exchange };

        // Act
        var executionPlan = _planner.CalculateExecutionPlan(OrderType.Sell, orderAmount, exchanges);

        // Assert
        var executionPlanOrders = executionPlan.Orders.ToList();

        // Expect a single order that uses the available Crypto exactly
        executionPlanOrders.Count.ShouldBe(1);

        executionPlanOrders[0].ExchangeId.ShouldBe(exchange.Id);
        executionPlanOrders[0].Price.ShouldBe(3100);
        executionPlanOrders[0].Amount.ShouldBe(1);
    }

    [Test]
    [Description("Should correctly allocate buy executionPlanOrders when two exchanges have identical ask prices.")]
    public void It_should_handle_tie_breaker_for_buy_orders_with_identical_ask_prices()
    {
        // Arrange
        // Both exchanges offer an ask at 3000
        // Exchange1: available Euro = 3000, so max BTC = 3000 / 3000 = 1 BTC (even if ask offers 2 BTC)
        // Exchange2: available Euro = 6000, so max BTC = 6000 / 3000 = 2 BTC
        var exchange1 = ExchangeBuilder.Create("ex1", crypto: 100, euro: 3000)
            .WithAsks(
            [
                (3000, 2)
            ])
            .Build();

        var exchange2 = ExchangeBuilder.Create("ex2", crypto: 100, euro: 6000)
            .WithAsks(
            [
                (3000, 2)
            ])
            .Build();

        decimal orderAmount = 2.5m; // Total BTC to buy
        var exchanges = new List<Exchange> { exchange1, exchange2 };

        // Act
        var executionPlan = _planner.CalculateExecutionPlan(OrderType.Buy, orderAmount, exchanges);

        // Assert
        var executionPlanOrders = executionPlan.Orders.ToList();

        // Expect the candidate from exchange1 (coming first in the list) to supply 1 BTC and then candidate from exchange2 to supply the remaining 1.5 BTC
        executionPlanOrders.Count.ShouldBe(2);

        executionPlanOrders[0].ExchangeId.ShouldBe(exchange1.Id);
        executionPlanOrders[0].Price.ShouldBe(3000);
        executionPlanOrders[0].Amount.ShouldBe(1);

        executionPlanOrders[1].ExchangeId.ShouldBe(exchange2.Id);
        executionPlanOrders[1].Price.ShouldBe(3000);
        executionPlanOrders[1].Amount.ShouldBe(1.5m);
    }

    [Test]
    [Description("Should correctly allocate sell executionPlanOrders when two exchanges have identical bid prices.")]
    public void It_should_handle_tie_breaker_for_sell_orders_with_identical_bid_prices()
    {
        // Arrange
        // Both exchanges offer a bid at 3100
        // Exchange1: available Crypto = 0.8 BTC, so can only sell 0.8 BTC even if bid offers 1 BTC
        // Exchange2: available Crypto = 1.5 BTC, so can sell up to 1.5 BTC
        var exchange1 = ExchangeBuilder.Create("ex1", crypto: 0.8m, euro: 100000)
            .WithBids(
            [
                (3100, 1)
            ])
            .Build();

        var exchange2 = ExchangeBuilder.Create("ex2", crypto: 1.5m, euro: 100000)
            .WithBids(
            [
                (3100, 1)
            ])
            .Build();

        decimal orderAmount = 1.2m; // Total BTC to sell
        var exchanges = new List<Exchange> { exchange1, exchange2 };

        // Act:
        var executionPlan = _planner.CalculateExecutionPlan(OrderType.Sell, orderAmount, exchanges);

        // Assert
        var executionPlanOrders = executionPlan.Orders.ToList();

        // Expect the candidate from exchange1 to supply 0.8 BTC (max available), then candidate from exchange2 to supply the remaining 0.4 BTC
        executionPlanOrders.Count.ShouldBe(2);

        executionPlanOrders[0].ExchangeId.ShouldBe(exchange1.Id);
        executionPlanOrders[0].Price.ShouldBe(3100);
        executionPlanOrders[0].Amount.ShouldBe(0.8m);

        executionPlanOrders[1].ExchangeId.ShouldBe(exchange2.Id);
        executionPlanOrders[1].Price.ShouldBe(3100);
        executionPlanOrders[1].Amount.ShouldBe(0.4m);
    }
}
