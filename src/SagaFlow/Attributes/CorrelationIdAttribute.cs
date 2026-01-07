namespace SagaFlow.Attributes;

/// <summary>
///     Marks a property as part of the correlation identifier for saga message routing.
///     Multiple properties can be marked to form a composite correlation identifier,
///     which will be concatenated using the specified separator.
/// </summary>
/// <remarks>
///     Apply this attribute to properties that together uniquely identify the saga instance.
///     The correlation ID links all messages belonging to the same saga instance.
/// </remarks>
/// <example>
///     <code>
/// public record OrderCreatedEvent : ISagaEvent
/// {
///     [CorrelationId(Order = 1)]
///     public Guid OrderId { get; init; }
///     
///     [CorrelationId(Order = 2)]
///     public string TenantId { get; init; }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property)]
public sealed class CorrelationIdAttribute : Attribute
{
    /// <summary>
    ///     Gets or sets the order of this property in the composite correlation identifier.
    ///     Lower values are processed first. Default is 0.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    ///     Gets or sets the separator used to join multiple properties in a composite correlation identifier.
    ///     Default is ":".
    /// </summary>
    public string Separator { get; set; } = ":";

    /// <summary>
    ///     Gets or sets a value indicating whether to include the property name in the identifier.
    ///     Default is <c>false</c>.
    /// </summary>
    public bool IncludePropertyName { get; set; }

    /// <summary>
    ///     Gets or sets the format string to use when converting the property value to string.
    ///     If not specified, <see cref="object.ToString" /> will be used.
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this correlation ID is required.
    ///     Default is <c>true</c>.
    /// </summary>
    public bool IsRequired { get; set; } = true;
}