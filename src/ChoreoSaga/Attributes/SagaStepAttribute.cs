namespace ChoreoSaga.Attributes;

/// <summary>
///     Marks a saga class as handling a specific saga message type.
///     <example>
///         [SagaStep(typeof(MyMessage), isInitiator: true, canBeInitiatorAndExecutor: true)]
///     </example>
///     <remarks>
///         The provided messageType must implement <see cref="ChoreoSaga.Messages.ISagaMessage" />.
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
    public SagaStepAttribute(Type messageType, bool isInitiator = false, bool canBeInitiatorAndExecutor = false)
    {
        MessageType = messageType;
        IsInitiator = isInitiator;
        CanBeInitiatorAndExecutor = canBeInitiatorAndExecutor;
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
}