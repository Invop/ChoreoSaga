using Microsoft.CodeAnalysis.Testing;
using Verify = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<ChoreoSaga.Analyzer.RetryPolicyAttributeAnalyzer, Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace ChoreoSaga.Analyzer.Tests;

/// <summary>
///     Tests for RetryPolicyAttributeAnalyzer to ensure proper validation of RetryPolicyAttribute usage.
/// </summary>
public class RetryPolicyAttributeAnalyzerTests
{
    private const string RetryPolicyBaseCode = """

                                               namespace ChoreoSaga.Attributes
                                               {
                                                   using System;

                                                   [AttributeUsage(AttributeTargets.Class)]
                                                   public sealed class RetryPolicyAttribute : Attribute
                                                   {
                                                       public int MaxRetries { get; set; } = 3;
                                                       public int RetryDelayMilliseconds { get; set; } = 1000;
                                                       public BackoffStrategy Strategy { get; set; } = BackoffStrategy.Exponential;
                                                       public bool UseJitter { get; set; } = true;
                                                       public Type[]? RetryOnExceptions { get; set; }
                                                       public Type[]? NonRetryableExceptions { get; set; }
                                                       public int TimeoutMilliseconds { get; set; }
                                                       public int MaxRetryDelayMilliseconds { get; set; } = 30000;
                                                       public bool EnableCircuitBreaker { get; set; }
                                                       public int CircuitBreakerThreshold { get; set; } = 5;
                                                       public int CircuitBreakerDurationSeconds { get; set; } = 60;
                                                   }

                                                   public enum BackoffStrategy
                                                   {
                                                       Constant,
                                                       Linear,
                                                       Exponential
                                                   }
                                               }

                                               """;

    [Fact]
    public async Task ValidRetryPolicyAttributeWithStandardExceptions_NoDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      [RetryPolicy(RetryOnExceptions = new[] { typeof(InvalidOperationException), typeof(TimeoutException) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ValidRetryPolicyAttributeWithNonRetryableExceptions_NoDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      [RetryPolicy(NonRetryableExceptions = new[] { typeof(ArgumentException), typeof(ArgumentNullException) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ValidRetryPolicyAttributeWithBothExceptionTypes_NoDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      [RetryPolicy(
                                                          RetryOnExceptions = new[] { typeof(InvalidOperationException) },
                                                          NonRetryableExceptions = new[] { typeof(ArgumentException) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ValidRetryPolicyAttributeWithCustomException_NoDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      public class OrderProcessingException : Exception
                                                      {
                                                      }

                                                      [RetryPolicy(RetryOnExceptions = new[] { typeof(OrderProcessingException) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ValidRetryPolicyAttributeWithInheritedCustomException_NoDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      public class OrderProcessingException : Exception
                                                      {
                                                      }

                                                      public class OrderTimeoutException : OrderProcessingException
                                                      {
                                                      }

                                                      [RetryPolicy(RetryOnExceptions = new[] { typeof(OrderTimeoutException) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task RetryOnExceptionsWithNonExceptionType_ReportsDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      public class InvalidExceptionType
                                                      {
                                                      }

                                                      [RetryPolicy(RetryOnExceptions = new[] { typeof(InvalidExceptionType) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        DiagnosticResult expected = Verify.Diagnostic(RetryPolicyAttributeAnalyzer.RetryOnExceptionsRuleDiagnosticId)
            .WithLocation(39, 46)
            .WithArguments("TestNamespace.InvalidExceptionType");

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task NonRetryableExceptionsWithNonExceptionType_ReportsDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      public class InvalidExceptionType
                                                      {
                                                      }

                                                      [RetryPolicy(NonRetryableExceptions = new[] { typeof(InvalidExceptionType) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        DiagnosticResult expected = Verify.Diagnostic(RetryPolicyAttributeAnalyzer.NonRetryableExceptionsRuleDiagnosticId)
            .WithLocation(39, 51)
            .WithArguments("TestNamespace.InvalidExceptionType");

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task BothExceptionPropertiesWithInvalidTypes_ReportsBothDiagnostics()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      public class InvalidRetryType
                                                      {
                                                      }

                                                      public class InvalidNonRetryType
                                                      {
                                                      }

                                                      [RetryPolicy(
                                                          RetryOnExceptions = new[] { typeof(InvalidRetryType) },
                                                          NonRetryableExceptions = new[] { typeof(InvalidNonRetryType) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        DiagnosticResult[] expected =
        {
            Verify.Diagnostic(RetryPolicyAttributeAnalyzer.RetryOnExceptionsRuleDiagnosticId)
                .WithLocation(44, 37)
                .WithArguments("TestNamespace.InvalidRetryType"),
            Verify.Diagnostic(RetryPolicyAttributeAnalyzer.NonRetryableExceptionsRuleDiagnosticId)
                .WithLocation(45, 42)
                .WithArguments("TestNamespace.InvalidNonRetryType")
        };

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task RetryOnExceptionsWithMultipleInvalidTypes_ReportsMultipleDiagnostics()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      public class InvalidType1
                                                      {
                                                      }

                                                      public class InvalidType2
                                                      {
                                                      }

                                                      [RetryPolicy(RetryOnExceptions = new[] { typeof(InvalidType1), typeof(InvalidType2) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        DiagnosticResult[] expected =
        {
            Verify.Diagnostic(RetryPolicyAttributeAnalyzer.RetryOnExceptionsRuleDiagnosticId)
                .WithLocation(43, 46)
                .WithArguments("TestNamespace.InvalidType1"),
            Verify.Diagnostic(RetryPolicyAttributeAnalyzer.RetryOnExceptionsRuleDiagnosticId)
                .WithLocation(43, 68)
                .WithArguments("TestNamespace.InvalidType2")
        };

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task RetryOnExceptionsWithMixedValidAndInvalidTypes_ReportsDiagnosticForInvalidOnly()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      public class InvalidType
                                                      {
                                                      }

                                                      [RetryPolicy(RetryOnExceptions = new[] { typeof(InvalidOperationException), typeof(InvalidType) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        DiagnosticResult expected = Verify.Diagnostic(RetryPolicyAttributeAnalyzer.RetryOnExceptionsRuleDiagnosticId)
            .WithLocation(39, 81)
            .WithArguments("TestNamespace.InvalidType");

        await Verify.VerifyAnalyzerAsync(test, expected);
    }

    [Fact]
    public async Task ValidRetryPolicyAttributeWithNoExceptionProperties_NoDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using ChoreoSaga.Attributes;

                                                      [RetryPolicy(MaxRetries = 5, RetryDelayMilliseconds = 2000)]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ValidRetryPolicyAttributeWithAllParameters_NoDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      [RetryPolicy(
                                                          MaxRetries = 5,
                                                          RetryDelayMilliseconds = 2000,
                                                          Strategy = BackoffStrategy.Linear,
                                                          UseJitter = false,
                                                          RetryOnExceptions = new[] { typeof(InvalidOperationException) },
                                                          NonRetryableExceptions = new[] { typeof(ArgumentException) },
                                                          TimeoutMilliseconds = 5000,
                                                          MaxRetryDelayMilliseconds = 10000,
                                                          EnableCircuitBreaker = true,
                                                          CircuitBreakerThreshold = 3,
                                                          CircuitBreakerDurationSeconds = 30)]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task RetryPolicyAttributeOnInterface_NoDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using ChoreoSaga.Attributes;

                                                      public interface IOrderSaga
                                                      {
                                                          void Process();
                                                      }
                                                  }
                                                  """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task RetryPolicyAttributeOnStruct_NoDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using ChoreoSaga.Attributes;

                                                      public struct OrderData
                                                      {
                                                          public string Id { get; set; }
                                                      }
                                                  }
                                                  """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task ValidRetryPolicyAttributeWithSystemIOException_NoDiagnostic()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using System.IO;
                                                      using ChoreoSaga.Attributes;

                                                      [RetryPolicy(RetryOnExceptions = new[] { typeof(IOException), typeof(FileNotFoundException) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        await Verify.VerifyAnalyzerAsync(test);
    }

    [Fact]
    public async Task NonRetryableExceptionsWithMultipleInvalidTypes_ReportsMultipleDiagnostics()
    {
        const string test = RetryPolicyBaseCode + """

                                                  namespace TestNamespace
                                                  {
                                                      using System;
                                                      using ChoreoSaga.Attributes;

                                                      public class InvalidType1
                                                      {
                                                      }

                                                      public class InvalidType2
                                                      {
                                                      }

                                                      [RetryPolicy(NonRetryableExceptions = new[] { typeof(InvalidType1), typeof(InvalidType2) })]
                                                      public class OrderSaga
                                                      {
                                                          public void Process()
                                                          {
                                                          }
                                                      }
                                                  }
                                                  """;

        DiagnosticResult[] expected =
        {
            Verify.Diagnostic(RetryPolicyAttributeAnalyzer.NonRetryableExceptionsRuleDiagnosticId)
                .WithLocation(43, 51)
                .WithArguments("TestNamespace.InvalidType1"),
            Verify.Diagnostic(RetryPolicyAttributeAnalyzer.NonRetryableExceptionsRuleDiagnosticId)
                .WithLocation(43, 73)
                .WithArguments("TestNamespace.InvalidType2")
        };

        await Verify.VerifyAnalyzerAsync(test, expected);
    }
}