using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SagaFlow.Analyzer;

/// <summary>
///     Analyzer that validates the correct usage of SagaStepAttribute.
///     Ensures that message types implement ISagaMessage and that the attribute is applied to ISaga implementations.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SagaStepAttributeAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    ///     Diagnostic ID for message type validation rule.
    /// </summary>
    public const string MessageTypeRuleDiagnosticId = "CHSG0001";

    /// <summary>
    ///     Diagnostic ID for saga class validation rule.
    /// </summary>
    public const string SagaClassRuleDiagnosticId = "CHSG0002";

    private const string SagaStepAttributeName = "SagaStepAttribute";
    private const string SagaMessageInterfaceName = "ISagaMessage";
    private const string SagaInterfaceName = "ISaga";

    // CHSG0001: Message type must implement ISagaMessage
    private static readonly LocalizableString MessageTypeRuleTitle = new LocalizableResourceString(
        nameof(Resources.CHSG0001Title),
        Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableString MessageTypeRuleMessageFormat = new LocalizableResourceString(
        nameof(Resources.CHSG0001MessageFormat),
        Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableString MessageTypeRuleDescription = new LocalizableResourceString(
        nameof(Resources.CHSG0001Description),
        Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor MessageTypeRule = new(
        MessageTypeRuleDiagnosticId,
        MessageTypeRuleTitle,
        MessageTypeRuleMessageFormat,
        "Usage",
        DiagnosticSeverity.Error,
        true,
        MessageTypeRuleDescription);

    // CHSG0002: Class must implement ISaga
    private static readonly LocalizableString SagaClassRuleTitle = new LocalizableResourceString(
        nameof(Resources.CHSG0002Title),
        Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableString SagaClassRuleMessageFormat = new LocalizableResourceString(
        nameof(Resources.CHSG0002MessageFormat),
        Resources.ResourceManager,
        typeof(Resources));

    private static readonly LocalizableString SagaClassRuleDescription = new LocalizableResourceString(
        nameof(Resources.CHSG0002Description),
        Resources.ResourceManager,
        typeof(Resources));

    private static readonly DiagnosticDescriptor SagaClassRule = new(
        SagaClassRuleDiagnosticId,
        SagaClassRuleTitle,
        SagaClassRuleMessageFormat,
        "Usage",
        DiagnosticSeverity.Error,
        true,
        SagaClassRuleDescription);

    /// <summary>
    ///     Gets the supported diagnostic descriptors for this analyzer.
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(MessageTypeRule, SagaClassRule);

    /// <summary>
    ///     Initializes the analyzer by registering analysis actions.
    /// </summary>
    /// <param name="context">Analysis context.</param>
    public override void Initialize(AnalysisContext context)
    {
        // Avoid analyzing generated code.
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

        // Enable concurrent execution for better performance.
        context.EnableConcurrentExecution();

        // Register semantic model action to analyze attributes on class declarations.
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    /// <summary>
    ///     Analyzes named types (classes) for SagaStepAttribute usage.
    /// </summary>
    /// <param name="context">Symbol analysis context.</param>
    private void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze classes.
        if (namedTypeSymbol.TypeKind != TypeKind.Class)
        {
            return;
        }

        // Get all attributes on the class.
        ImmutableArray<AttributeData> attributes = namedTypeSymbol.GetAttributes();

        foreach (AttributeData? attribute in attributes)
        {
            // Check if this is SagaStepAttribute.
            if (attribute.AttributeClass?.Name != SagaStepAttributeName)
            {
                continue;
            }

            // Validate that the class implements ISaga.
            if (!ImplementsInterface(namedTypeSymbol, SagaInterfaceName))
            {
                var diagnostic = Diagnostic.Create(
                    SagaClassRule,
                    attribute.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? namedTypeSymbol.Locations[0],
                    namedTypeSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }

            // Validate the message type parameter.
            if (attribute.ConstructorArguments.Length == 0)
            {
                continue;
            }

            TypedConstant messageTypeArgument = attribute.ConstructorArguments[0];
            if (messageTypeArgument.Kind != TypedConstantKind.Type ||
                messageTypeArgument.Value is not INamedTypeSymbol messageType)
            {
                continue;
            }

            // Check if the message type implements ISagaMessage.
            if (!ImplementsInterface(messageType, SagaMessageInterfaceName))
            {
                SyntaxNode? attributeSyntax = attribute.ApplicationSyntaxReference?.GetSyntax();
                Location location = attributeSyntax is AttributeSyntax attrSyntax &&
                                    attrSyntax.ArgumentList?.Arguments.Count > 0
                    ? attrSyntax.ArgumentList.Arguments[0].GetLocation()
                    : attributeSyntax?.GetLocation() ?? namedTypeSymbol.Locations[0];

                var diagnostic = Diagnostic.Create(
                    MessageTypeRule,
                    location,
                    messageType.Name);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    /// <summary>
    ///     Checks if a type implements a specific interface by name.
    /// </summary>
    /// <param name="typeSymbol">The type to check.</param>
    /// <param name="interfaceName">The interface name to look for.</param>
    /// <returns>True if the type implements the interface, false otherwise.</returns>
    private static bool ImplementsInterface(INamedTypeSymbol typeSymbol, string interfaceName)
    {
        return typeSymbol.AllInterfaces.Any(i => i.Name == interfaceName);
    }
}