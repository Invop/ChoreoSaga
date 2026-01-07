namespace SagaFlow.Messages;

/// <summary>
///     Defines a handler for saga commands.
///     Command handlers are typically used by participant services to execute business operations.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle</typeparam>
public interface ISagaCommandHandler<in TCommand> where TCommand : ISagaCommand
{
    /// <summary>
    ///     Handles the saga command asynchronously.
    /// </summary>
    /// <param name="context">The message context containing the command and metadata</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A ValueTask representing the async operation</returns>
    ValueTask HandleAsync(ISagaMessageContext<TCommand> context, CancellationToken cancellationToken = default);
}