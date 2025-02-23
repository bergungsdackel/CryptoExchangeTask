using CryptoExchangeTask.Domain.Entities;

namespace CryptoExchangeTask.Domain.Abstractions;

/// <summary>
/// Provides methods to interact with the exchange repository.
/// </summary>
public interface IExchangeRepository
{
    /// <summary>
    /// Retrieves all exchanges with their order books and fund storage.
    /// </summary>
    /// <returns>A collection of exchanges.</returns>
    IEnumerable<Exchange> GetAllExchanges();
}
