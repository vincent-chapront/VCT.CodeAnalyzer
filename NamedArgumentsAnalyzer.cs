using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NamedArgumentsAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NamedArgumentsAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor RuleMultipleArgumentsShouldBeNamed = new(
            id: "VCT0001",
            title: "Named arguments required",
            messageFormat: "This argument should be named. For method calls with multiple arguments, the arguments should all be named.",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "For method calls with multiple arguments, the arguments should all be named."
        );
        private static readonly DiagnosticDescriptor RuleSingleArgumentsShouldNotBeNamed = new(
            id: "VCT0002",
            title: "Argument should not be named",
            messageFormat: "This argument should not be named. For method calls with only one argument, the argument should not be named.",
            category: "Style",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "For method calls with only one argument, the argument should not be named."
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RuleMultipleArgumentsShouldBeNamed,RuleSingleArgumentsShouldNotBeNamed];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            var args = invocation.ArgumentList?.Arguments;
            if (args == null || args.Value.Count == 0)
            {
                return;
            }
            else if (args.Value.Count == 1)
            {
                if (args.Value[0].NameColon != null)
                {
                    var diagnostic = Diagnostic.Create(RuleSingleArgumentsShouldNotBeNamed, args.Value[0].GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
            else
            {
                foreach (var arg in args)
                {
                    if (arg.NameColon == null)
                    {
                        var diagnostic = Diagnostic.Create(RuleMultipleArgumentsShouldBeNamed, arg.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }
    }
}
