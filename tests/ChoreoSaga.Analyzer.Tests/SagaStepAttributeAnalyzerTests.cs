using Microsoft.CodeAnalysis.Testing;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<ChoreoSaga.Analyzer.SagaStepAttributeAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace ChoreoSaga.Analyzer.Tests;

/// <summary>
///     Tests for SagaStepAttributeAnalyzer to ensure proper validation of SagaStepAttribute usage.
/// </summary>
public class SagaStepAttributeAnalyzerTests
{
    private const string SagaBaseCode = """

                                        namespace ChoreoSaga.Messages
                                        {
                                            public interface ISagaMessage
                                            {
                                                string CorrelationId { get; }
                                            }
                                        }

                                        namespace ChoreoSaga.Sagas
                                        {
                                            public interface ISaga
                                            {
                                                string CorrelationId { get; set; }
                                            }
                                        }

                                        namespace ChoreoSaga.Attributes
                                        {
                                            using System;

                                            [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
                                            public sealed class SagaStepAttribute : Attribute
                                            {
                                                public SagaStepAttribute(Type messageType, bool isInitiator = false, bool canBeInitiatorAndExecutor = false)
                                                {
                                                    MessageType = messageType;
                                                    IsInitiator = isInitiator;
                                                    CanBeInitiatorAndExecutor = canBeInitiatorAndExecutor;
                                                }

                                                public Type MessageType { get; }
                                                public bool IsInitiator { get; }
                                                public bool CanBeInitiatorAndExecutor { get; }
                                            }
                                        }

                                        """;

    [Fact]
    public async Task ValidSagaStepAttribute_NoDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using ChoreoSaga.Attributes;
                                               using ChoreoSaga.Messages;
                                               using ChoreoSaga.Sagas;

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
                                               using ChoreoSaga.Attributes;
                                               using ChoreoSaga.Messages;
                                               using ChoreoSaga.Sagas;

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
                                               using ChoreoSaga.Attributes;
                                               using ChoreoSaga.Sagas;

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
            .WithLocation(48, 15)
            .WithArguments("InvalidMessage");

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ClassDoesNotImplementISaga_ReportsDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using ChoreoSaga.Attributes;
                                               using ChoreoSaga.Messages;

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
            .WithLocation(48, 6)
            .WithArguments("InvalidSaga");

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task BothInvalidMessageAndInvalidSaga_ReportsBothDiagnostics()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using ChoreoSaga.Attributes;

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
                .WithLocation(47, 6)
                .WithArguments("InvalidSaga"),
            Verify.Diagnostic(SagaStepAttributeAnalyzer.MessageTypeRuleDiagnosticId)
                .WithLocation(47, 15)
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
                                               using ChoreoSaga.Attributes;
                                               using ChoreoSaga.Messages;
                                               using ChoreoSaga.Sagas;

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
            .WithLocation(55, 15)
            .WithArguments("InvalidMessage");

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task SagaImplementingISagaThroughBaseClass_NoDiagnostic()
    {
        const string test = SagaBaseCode + """

                                           namespace TestNamespace
                                           {
                                               using ChoreoSaga.Attributes;
                                               using ChoreoSaga.Messages;
                                               using ChoreoSaga.Sagas;

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
                                               using ChoreoSaga.Attributes;
                                               using ChoreoSaga.Messages;
                                               using ChoreoSaga.Sagas;

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
                                               using ChoreoSaga.Attributes;
                                               using ChoreoSaga.Messages;
                                               using ChoreoSaga.Sagas;

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
}