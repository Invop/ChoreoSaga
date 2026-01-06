namespace ChoreoSaga.Messages;

/// <summary>
///     Interface for saga commands.
///     Commands represent actions that should be executed as part of a saga step.
///     They are typically sent to a specific service to perform an operation.
/// </summary>
public interface ISagaCommand : ISagaMessage
{
    /// <summary>
    ///     Gets the idempotency key to ensure the command is processed only once.
    /// </summary>
    string IdempotencyKey { get; }
}