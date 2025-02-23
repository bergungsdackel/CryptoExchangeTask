using CryptoExchangeTask.Domain.Abstractions;
using CryptoExchangeTask.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CryptoExchangeTask.Infrastructure.Data;

public class JsonExchangeRepository : IExchangeRepository
{
    private readonly ILogger<JsonExchangeRepository> _logger;

    private readonly string _jsonFileDirectory;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonExchangeRepository(string jsonFileDirectory, ILogger<JsonExchangeRepository> logger)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonFileDirectory, nameof(jsonFileDirectory));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        if (!Directory.Exists(jsonFileDirectory))
        {
            throw new DirectoryNotFoundException($"The exchanges JSON file directory does not exist: {jsonFileDirectory}");
        }

        _jsonFileDirectory = jsonFileDirectory;
        _logger = logger;
    }

    /// <inheritdoc/>
    public IEnumerable<Exchange> GetAllExchanges()
    {
        var exchangeFiles = Directory.EnumerateFiles(_jsonFileDirectory, "exchange-*.json");
        var exchanges = new List<Exchange>();

        foreach (var exchangeFile in exchangeFiles)
        {
            try
            {
                var jsonData = File.ReadAllText(exchangeFile);
                var exchange = JsonSerializer.Deserialize<Exchange>(jsonData, _jsonSerializerOptions);
                if (exchange is not null)
                {
                    exchanges.Add(exchange);
                }
                else
                {
                    _logger.LogWarning("Deserialization returned null for file: {ExchangeFile}", exchangeFile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing file: {ExchangeFile}", exchangeFile);
            }
        }

        if (exchanges.Count == 0)
        {
            throw new InvalidOperationException("No exchanges could be deserialized from the JSON files.");
        }

        return exchanges;
    }
}
