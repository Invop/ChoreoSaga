using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ChoreoSaga.Analyzer;

//TODO: Tests
/// <summary>
///     Code fix provider for SagaStepAttribute validation errors.
///     Provides fixes for implementing required interfaces (ISaga and ISagaMessage).
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(SagaStepAttributeCodeFixProvider))]
[Shared]
public class SagaStepAttributeCodeFixProvider : CodeFixProvider
{
    /// <summary>
    ///     Gets the fixable diagnostic IDs.
    /// </summary>
    public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(
            SagaStepAttributeAnalyzer.MessageTypeRuleDiagnosticId,
            SagaStepAttributeAnalyzer.SagaClassRuleDiagnosticId);

    /// <summary>
    ///     Gets the fix all provider.
    /// </summary>
    /// <returns>Fix all provider instance.</returns>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>
    ///     Registers code fixes for the specified context.
    /// </summary>
    /// <param name="context">Code fix context.</param>
    /// <returns>Async task.</returns>
    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        SyntaxNode? root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        Diagnostic diagnostic = context.Diagnostics.First();
        TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

        // Find the attribute or class declaration identified by the diagnostic.
        SyntaxNode node = root.FindNode(diagnosticSpan);

        if (diagnostic.Id == SagaStepAttributeAnalyzer.SagaClassRuleDiagnosticId)
        {
            // Find the class declaration that has the attribute.
            ClassDeclarationSyntax? classDeclaration = node.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            if (classDeclaration is not null)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Implement ISaga interface",
                        c => AddISagaInterfaceAsync(context.Document, classDeclaration, c),
                        nameof(SagaStepAttributeAnalyzer.SagaClassRuleDiagnosticId)),
                    diagnostic);
            }
        }
        else if (diagnostic.Id == SagaStepAttributeAnalyzer.MessageTypeRuleDiagnosticId)
        {
            // For message type validation, we would need to navigate to the type definition,
            // which is more complex. For now, we provide a descriptive message.
            // This could be expanded in the future to navigate to the type and add ISagaMessage.
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Navigate to message type to implement ISagaMessage",
                    _ => Task.FromResult(context.Document),
                    nameof(SagaStepAttributeAnalyzer.MessageTypeRuleDiagnosticId)),
                diagnostic);
        }
    }

    /// <summary>
    ///     Adds ISaga interface to the class declaration.
    /// </summary>
    /// <param name="document">The document to modify.</param>
    /// <param name="classDeclaration">The class declaration to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated document.</returns>
    private async Task<Document> AddISagaInterfaceAsync(
        Document document,
        ClassDeclarationSyntax classDeclaration,
        CancellationToken cancellationToken)
    {
        SyntaxNode? root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        // Create the ISaga interface type.
        SimpleBaseTypeSyntax sagaInterface = SyntaxFactory.SimpleBaseType(
            SyntaxFactory.ParseTypeName("ChoreoSaga.Sagas.ISaga"));

        // Add the interface to the base list.
        BaseListSyntax newBaseList = classDeclaration.BaseList is null
            ? SyntaxFactory.BaseList(
                SyntaxFactory.SingletonSeparatedList<BaseTypeSyntax>(sagaInterface))
            : classDeclaration.BaseList.AddTypes(sagaInterface);

        ClassDeclarationSyntax newClassDeclaration = classDeclaration.WithBaseList(newBaseList);

        // Replace the old class declaration with the new one.
        SyntaxNode newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

        return document.WithSyntaxRoot(newRoot);
    }
}