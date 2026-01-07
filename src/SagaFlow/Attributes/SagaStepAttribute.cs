using SagaFlow.Messages;

namespace SagaFlow.Attributes;

/// <summary>
///     Marks a saga class as handling a specific saga message type.
///     <example>
///         [SagaStep(typeof(MyMessage), isInitiator: true, canBeInitiatorAndExecutor: true)]
///     </example>
///     <remarks>
///         The provided messageType must implement <see cref="ISagaMessage" />.
///         This is validated at runtime when registering saga steps.
///         Set isInitiator to true if the message initiates the saga.
///         Set canBeInitiatorAndExecutor to true if the message can act as both initiator and executor.
///     </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SagaStepAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SagaStepAttribute" /> class.
    /// </summary>
    /// <param name="messageType">Type of the saga message to be handled. Must implement ISagaMessage.</param>
    /// <param name="isInitiator">Set to true if the message initiates the saga. Default is false.</param>
    /// <param name="canBeInitiatorAndExecutor">
    ///     Set to true if the message can act as both initiator and executor. Default is
    ///     false.
    /// </param>
    /// <param name="isPivot">
    ///     Set to true if this step is the pivot point of the saga. A pivot point represents the point of no
    ///     return - after it executes, compensating transactions are no longer relevant. A saga can have at most one pivot
    ///     point. Default is false.
    /// </param>
    public SagaStepAttribute(Type messageType, bool isInitiator = false, bool canBeInitiatorAndExecutor = false,
        bool isPivot = false)
    {
        MessageType = messageType;
        IsInitiator = isInitiator;
        CanBeInitiatorAndExecutor = canBeInitiatorAndExecutor;
        IsPivot = isPivot;
    }

    /// <summary>
    ///     Gets the type of the saga message handled by the saga step.
    /// </summary>
    public Type MessageType { get; }

    /// <summary>
    ///     Gets a value indicating whether the message initiates the saga.
    /// </summary>
    public bool IsInitiator { get; }

    /// <summary>
    ///     Gets a value indicating whether the message can act as both initiator and executor.
    /// </summary>
    public bool CanBeInitiatorAndExecutor { get; }

    /// <summary>
    ///     Gets a value indicating whether this step is the pivot point of the saga.
    ///     A pivot point represents the point of no return in a saga - after it executes,
    ///     compensating transactions are no longer relevant. A saga can have at most one pivot point.
    /// </summary>
    public bool IsPivot { get; }
}