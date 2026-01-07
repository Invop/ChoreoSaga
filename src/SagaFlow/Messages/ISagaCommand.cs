namespace SagaFlow.Messages;

/// <summary>
///     Interface for saga commands.
///     Commands represent actions that should be executed as part of a saga step.
///     They are typically sent to a specific service to perform an operation.
/// </summary>
public interface ISagaCommand : ISagaMessage;