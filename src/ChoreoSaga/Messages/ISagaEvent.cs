namespace ChoreoSaga.Messages;

/// <summary>
///     Interface for saga events.
///     Events represent the outcome of a saga step execution.
///     They notify the orchestrator about the success or failure of an operation.
/// </summary>
public interface ISagaEvent : ISagaMessage
{
    /// <summary>
    ///     Gets the identifier of the saga step that produced this event.
    /// </summary>
    string StepId { get; }

    /// <summary>
    ///     Gets a value indicating whether the step completed successfully.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    ///     Gets the error message if the step failed; otherwise, null.
    /// </summary>
    string? ErrorMessage { get; }
}