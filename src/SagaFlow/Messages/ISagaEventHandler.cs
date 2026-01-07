using SagaFlow.Sagas;

namespace SagaFlow.Messages;

/// <summary>
///     Defines a handler for saga events with access to saga state.
///     Event handlers are used by saga orchestrators to process step outcomes and update saga flow.
/// </summary>
/// <typeparam name="TEvent">The type of event to handle</typeparam>
/// <typeparam name="TState">The type of saga state</typeparam>
public interface ISagaEventHandler<in TEvent, in TState> where TEvent : ISagaEvent
{
    /// <summary>
    ///     Handles the saga event asynchronously with access to the saga context.
    /// </summary>
    /// <param name="context">The message context containing the event and metadata</param>
    /// <param name="sagaContext">The saga context providing access to the current saga state</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>A ValueTask representing the async operation</returns>
    ValueTask HandleAsync(
        ISagaMessageContext<TEvent> context,
        ISagaContext<TState> sagaContext,
        CancellationToken cancellationToken = default);
}