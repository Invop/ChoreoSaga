namespace SagaFlow.Attributes;

/// <summary>
///     Specifies the concurrency mode for a saga.
/// </summary>
public enum SagaConcurrencyMode
{
    /// <summary>
    ///     Optimistic concurrency: assumes conflicts are rare, checks for conflicts at commit time.
    /// </summary>
    Optimistic,

    /// <summary>
    ///     Pessimistic concurrency: locks the resource for the duration of the saga execution.
    /// </summary>
    Pessimistic
}

/// <summary>
///     Attribute for specifying the concurrency mode of a saga.
///     <example>
///         [SagaConcurrency(SagaConcurrencyMode.Pessimistic)]
///         public class MySaga { /* ... */ }
///     </example>
///     <remarks>
///         Apply only to classes that manage sagas. Allows explicit selection of concurrency mode: optimistic or pessimistic.
///         By default, <see cref="SagaConcurrencyMode.Optimistic" /> is used.
///     </remarks>
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SagaConcurrencyAttribute : Attribute
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SagaConcurrencyAttribute" /> class.
    /// </summary>
    /// <param name="mode">The concurrency mode for the saga. Default is Optimistic.</param>
    public SagaConcurrencyAttribute(SagaConcurrencyMode mode = SagaConcurrencyMode.Optimistic)
    {
        Mode = mode;
    }

    /// <summary>
    ///     The concurrency mode for the saga.
    /// </summary>
    public SagaConcurrencyMode Mode { get; }
}