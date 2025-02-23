namespace CryptoExchangeTask.Domain.Entities;

/// <summary>
/// Represents an execution plan in the crypto exchange.
/// </summary>
public class ExecutionPlan
{
    /// <summary>
    /// Gets the total cost of the execution plan.
    /// </summary>
    public decimal TotalCost { get; private set; } = 0;

    /// <summary>
    /// Gets the collection of execution orders.
    /// </summary>
    public ICollection<ExecutionOrder> Orders { get; init; } = [];

    /// <summary>
    /// Adds an execution order to the plan and updates the total cost.
    /// </summary>
    /// <param name="order">The execution order to add.</param>
    public void AddOrder(ExecutionOrder order)
    {
        Orders.Add(order);
        TotalCost += order.Price * order.Amount;
    }
}
