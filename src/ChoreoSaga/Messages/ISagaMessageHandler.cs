namespace ChoreoSaga.Messages;

/// <summary>
///     Defines a contract for handling specific types of saga messages within the saga orchestration framework.
///     Implementations of this interface are responsible for processing incoming messages and updating saga state
///     accordingly.
/// </summary>
/// <typeparam name="TSagaMessage">
///     The specific type of saga message this handler processes, must implement
///     <see cref="ISagaMessage" />
/// </typeparam>
public interface ISagaMessageHandler<in TSagaMessage> where TSagaMessage : ISagaMessage
{
    /// <summary>
    ///     Handles the processing of a saga message asynchronously.
    /// </summary>
    /// <param name="context">The message context containing the message and associated metadata</param>
    /// <param name="cancellationToken">Optional cancellation token to support operation cancellation</param>
    /// <returns>A <see cref="ValueTask" /> representing the asynchronous message processing operation</returns>
    /// <remarks>
    ///     This method should contain all the business logic for processing the specific message type.
    ///     The implementation should be idempotent when possible, as messages may be reprocessed in case of failures.
    /// </remarks>
    ValueTask HandleAsync(IMessageContext<TSagaMessage> context, CancellationToken cancellationToken = default);
}