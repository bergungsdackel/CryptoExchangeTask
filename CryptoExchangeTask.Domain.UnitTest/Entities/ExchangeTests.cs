using CryptoExchangeTask.Domain.Entities;
using CryptoExchangeTask.Domain.Enums;
using Shouldly;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchangeTask.Domain.UnitTest.Entities
{
    public class ExchangeTests
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        [Test]
        public void It_should_deserialize_exchanges_from_json_file()
        {
            // Arrange
            var jsonFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "exchange-test.json");
            File.Exists(jsonFilePath).ShouldBeTrue($"The JSON file should exist at: {jsonFilePath}");
            var jsonString = File.ReadAllText(jsonFilePath);

            // Act
            var exchange = JsonSerializer.Deserialize<Exchange>(jsonString, _jsonSerializerOptions);

            // Validate Exchange
            exchange.ShouldNotBeNull();
            exchange.Id.ShouldBe("exchange-test");

            // Validate AvailableFunds
            exchange.AvailableFunds.ShouldNotBeNull();
            exchange.AvailableFunds.Crypto.ShouldBe(1337.133m);
            exchange.AvailableFunds.Euro.ShouldBe(420.11m);

            // Validate OrderBook counts
            exchange.OrderBook.ShouldNotBeNull();
            exchange.OrderBook.Bids.ShouldNotBeNull();
            exchange.OrderBook.Asks.ShouldNotBeNull();
            exchange.OrderBook.Bids.Count.ShouldBe(2);
            exchange.OrderBook.Asks.Count.ShouldBe(2);

            // Validate first Bid Order
            var bid1 = exchange.OrderBook.Bids.ElementAt(0).Order;
            bid1.Id.ToString().ShouldBe("6e9fe255-a776-4965-9bf4-9f076361f5cb");
            bid1.Time.ShouldBe(DateTime.Parse("2024-03-01T14:41:06.563Z").ToUniversalTime());
            bid1.Type.ShouldBe(OrderType.Buy);
            bid1.Kind.ShouldBe(OrderKind.Limit);
            bid1.Amount.ShouldBe(0.01m);
            bid1.Price.ShouldBe(57226.46m);

            // Validate second Bid Order
            var bid2 = exchange.OrderBook.Bids.ElementAt(1).Order;
            bid2.Id.ToString().ShouldBe("86b69db0-b1cb-49d5-beb7-7068cfcbe14d");
            bid2.Time.ShouldBe(DateTime.Parse("2024-03-01T21:11:09.439Z").ToUniversalTime());
            bid2.Type.ShouldBe(OrderType.Buy);
            bid2.Kind.ShouldBe(OrderKind.Limit);
            bid2.Amount.ShouldBe(0.5m);
            bid2.Price.ShouldBe(57226.08m);

            // Validate first Ask Order
            var ask1 = exchange.OrderBook.Asks.ElementAt(0).Order;
            ask1.Id.ToString().ShouldBe("719f85c9-163e-471c-8edd-67021cfef195");
            ask1.Time.ShouldBe(DateTime.Parse("2024-03-01T00:46:50.389Z").ToUniversalTime());
            ask1.Type.ShouldBe(OrderType.Sell);
            ask1.Kind.ShouldBe(OrderKind.Limit);
            ask1.Amount.ShouldBe(0.405m);
            ask1.Price.ShouldBe(57299.73m);

            // Validate second Ask Order
            var ask2 = exchange.OrderBook.Asks.ElementAt(1).Order;
            ask2.Id.ToString().ShouldBe("4d8d9414-399a-4ceb-8d09-2af7e4db6000");
            ask2.Time.ShouldBe(DateTime.Parse("2024-03-01T09:46:16.308Z").ToUniversalTime());
            ask2.Type.ShouldBe(OrderType.Sell);
            ask2.Kind.ShouldBe(OrderKind.Limit);
            ask2.Amount.ShouldBe(0.405m);
            ask2.Price.ShouldBe(57299.92m);
        }
    }
}