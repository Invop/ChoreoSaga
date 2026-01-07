using Microsoft.CodeAnalysis.Testing;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<SagaFlow.Analyzer.SagaStepAttributeAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace SagaFlow.Analyzer.Tests;

/// <summary>
///     Tests for SagaStepAttributeAnalyzer to ensure proper validation of SagaStepAttribute usage.
/// </summary>
public class SagaStepAttributeAnalyzerTests
{
    private const string SagaBaseCode = """

                                        namespace SagaFlow.Messages
                                        {
                                            public interface ISagaMessage
                                            {
                                                string CorrelationId { get; }
                                            }
                                        }

                                        namespace SagaFlow.Sagas
                                        {
                                            public interface ISaga
                                            {
                                                string CorrelationId { get; set; }
                                            }
                                        }

                                        namespace SagaFlow.Attributes
                                        {
                                            using System;

                                            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                            public sealed class SagaStepAttribute : Attribute
                                            {
                                                public SagaStepAttribute(Type messageType, bool isInitiator = false, bool canBeInitiatorAndExecutor = false, bool isPivot = false)
                                                {
                                                    MessageType = messageType;
                                                    IsInitiator = isInitiator;
                                                    CanBeInitiatorAndExecutor = canBeInitiatorAndExecutor;
                                                    IsPivot = isPivot;
                                                }

                                                public Type MessageType { get; }
                                                public bool IsInitiator { get; }
                                                public bool CanBeInitiatorAndExecutor { get; }
                                                public bool IsPivot { get; }
                                            }
                                        }

                                        """;

    [Fact]
    public async Task ValidSagaStepAttribute_NoDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent))]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ValidSagaStepAttributeWithMultipleAttributes_NoDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               public class OrderCompletedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent), isInitiator: true)]
                                               [SagaStep(typeof(OrderCompletedEvent))]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MessageTypeDoesNotImplementISagaMessage_ReportsDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Sagas;

                                               public class InvalidMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(InvalidMessage))]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        DiagnosticResult expected = Verify.Diagnostic(SagaStepAttributeAnalyzer.MessageTypeRuleDiagnosticId)
            .WithLocation(50, 15)
            .WithArguments("InvalidMessage");

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ClassDoesNotImplementISaga_ReportsDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent))]
                                               public class InvalidSaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        DiagnosticResult expected = Verify.Diagnostic(SagaStepAttributeAnalyzer.SagaClassRuleDiagnosticId)
            .WithLocation(50, 6)
            .WithArguments("InvalidSaga");

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task BothInvalidMessageAndInvalidSaga_ReportsBothDiagnostics()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;

                                               public class InvalidMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(InvalidMessage))]
                                               public class InvalidSaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        DiagnosticResult[] expected =
        {
            Verify.Diagnostic(SagaStepAttributeAnalyzer.SagaClassRuleDiagnosticId)
                .WithLocation(49, 6)
                .WithArguments("InvalidSaga"),
            Verify.Diagnostic(SagaStepAttributeAnalyzer.MessageTypeRuleDiagnosticId)
                .WithLocation(49, 15)
                .WithArguments("InvalidMessage")
        };

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task MultipleAttributesWithOneInvalidMessage_ReportsSingleDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               public class InvalidMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent), isInitiator: true)]
                                               [SagaStep(typeof(InvalidMessage))]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        DiagnosticResult expected = Verify.Diagnostic(SagaStepAttributeAnalyzer.MessageTypeRuleDiagnosticId)
            .WithLocation(57, 15)
            .WithArguments("InvalidMessage");

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task SagaImplementingISagaThroughBaseClass_NoDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               public abstract class BaseSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent))]
                                               public class OrderSaga : BaseSaga
                                               {
                                               }
                                           }
                                           """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MessageImplementingISagaMessageThroughBaseInterface_NoDiagnostic()
    {
        const string test = SagaBaseCode + """
                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public interface IOrderMessage : ISagaMessage
                                               {
                                               }

                                               public class OrderCreatedEvent : IOrderMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent))]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task AttributeWithAllParameters_ValidSaga_NoDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent), isInitiator: true, canBeInitiatorAndExecutor: true)]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task SinglePivotPoint_NoDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               public class PaymentProcessedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent), isInitiator: true)]
                                               [SagaStep(typeof(PaymentProcessedEvent), isPivot: true)]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NoPivotPoint_NoDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               public class PaymentProcessedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent), isInitiator: true)]
                                               [SagaStep(typeof(PaymentProcessedEvent))]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task MultiplePivotPoints_ReportsDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               public class PaymentProcessedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               public class InventoryReservedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent), isInitiator: true)]
                                               [SagaStep(typeof(PaymentProcessedEvent), isPivot: true)]
                                               [SagaStep(typeof(InventoryReservedEvent), isPivot: true)]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        var expected = Verify.Diagnostic(SagaStepAttributeAnalyzer.MultiplePivotPointsRuleDiagnosticId)
            .WithLocation(63, 6)
            .WithArguments("OrderSaga", 2);

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ThreePivotPoints_ReportsDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               public class PaymentProcessedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               public class InventoryReservedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               public class ShipmentInitiatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent), isInitiator: true, isPivot: true)]
                                               [SagaStep(typeof(PaymentProcessedEvent), isPivot: true)]
                                               [SagaStep(typeof(InventoryReservedEvent), isPivot: true)]
                                               [SagaStep(typeof(ShipmentInitiatedEvent))]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        var expected = Verify.Diagnostic(SagaStepAttributeAnalyzer.MultiplePivotPointsRuleDiagnosticId)
            .WithLocation(67, 6)
            .WithArguments("OrderSaga", 3);

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task PivotPointWithNamedParameter_NoDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using SagaFlow.Attributes;
                                               using SagaFlow.Messages;
                                               using SagaFlow.Sagas;

                                               public class OrderCreatedEvent : ISagaMessage
                                               {
                                                   public string CorrelationId { get; set; }
                                               }

                                               [SagaStep(typeof(OrderCreatedEvent), isInitiator: true, isPivot: true)]
                                               public class OrderSaga : ISaga
                                               {
                                                   public string CorrelationId { get; set; }
                                               }
                                           }
                                           """;

        await Verify.VerifyAnalyzerAsync(test);
    }
}