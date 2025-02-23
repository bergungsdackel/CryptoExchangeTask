namespace CryptoExchangeTask.Domain.Exceptions;

/// <summary>
/// Exception thrown when there is insufficient liquidity to execute an order.
/// </summary>
public class InsufficientLiquidityException : Exception
{
    public InsufficientLiquidityException()
        : base("Insufficient liquidity to execute the order.")
    {
    }

    public InsufficientLiquidityException(string message)
        : base(message)
    {
    }

    public InsufficientLiquidityException(string message, Exception inner)
        : base(message, inner)
    {
    }
}