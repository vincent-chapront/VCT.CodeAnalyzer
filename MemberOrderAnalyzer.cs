using System.Collections.Immutable;
using System.Reflection.Metadata.Ecma335;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodeAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemberOrderAnalyzer : DiagnosticAnalyzer
    {
        enum ElementScope
        {
            Unknown=-1,
            Public = 1,
            Protected=2,
            Internal=3,
            Private=4,
        }

        enum ElementType
        {
            Unknown=-1,
            Field=1,
            Event=2,
            Constructor=3,
            Property=4,
            Method=5,
        }
        
        private static readonly DiagnosticDescriptor RuleByScope = new(
            "VCT0010",
            "Class members are not in the correct order",
            "`{0}` should appear before `{1}`",
            "Ordering",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Enforces ordering of class members by scope (public, protected, internal, private)."
            );
        
        private static readonly DiagnosticDescriptor RuleByType = new(
            "VCT0011",
            "Class members are not in the correct order",
            "`{0}` should appear before `{1}`",
            "Ordering",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Enforces ordering of class members by type (fields, constructor, properties, events, methods).");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [RuleByScope, RuleByType];

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;

            var members = classDecl.Members;

            ElementScope lastFieldAccessLevel = ElementScope.Unknown;
            ElementType lastElementType = ElementType.Unknown;

            foreach (var member in members)
            {
                if (member is FieldDeclarationSyntax
                || member is PropertyDeclarationSyntax
                || member is MethodDeclarationSyntax
                || member is EventDeclarationSyntax
                || member is ConstructorDeclarationSyntax)
                {
                    ElementScope accessLevel = GetAccessLevel(member);
                    ElementType elementType = GetElementType(member);
                    
                    if (accessLevel < lastFieldAccessLevel && elementType == lastElementType)
                    {
                        ReportDiagnosticScope(context, member, accessLevel.ToString(), lastFieldAccessLevel.ToString());
                    }
                    
                    if (elementType < lastElementType)
                    {
                        ReportDiagnosticType(context, member, elementType.ToString(), lastElementType.ToString());
                    }

                    lastFieldAccessLevel = accessLevel;
                    lastElementType = elementType;
                }
            }
        }

        private static ElementScope GetAccessLevel(MemberDeclarationSyntax member)
        {
            if (member.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                return ElementScope.Public;
            }

            else if (member.Modifiers.Any(SyntaxKind.InternalKeyword))
            {
                return ElementScope.Internal;
            }

            else if (member.Modifiers.Any(SyntaxKind.ProtectedKeyword))
            {
                return ElementScope.Protected;
            }

            else if (member.Modifiers.Any(SyntaxKind.PrivateKeyword))
            {
                return ElementScope.Private;
            }

            return ElementScope.Unknown;
        }

        private static ElementType GetElementType(MemberDeclarationSyntax member)
        {
            if (member is FieldDeclarationSyntax)
            {
                return ElementType.Field;
            }

            else if (member is PropertyDeclarationSyntax)
            {
                return ElementType.Property;
            }

            else if (member is MethodDeclarationSyntax)
            {
                return ElementType.Method;
            }

            else if (member is EventDeclarationSyntax)
            {
                return ElementType.Event;
            }

            else if (member is ConstructorDeclarationSyntax)
            {
                return ElementType.Constructor;
            }

            return ElementType.Unknown;
        }

        private static void ReportDiagnosticScope(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax member, string first, string second)
        {
            var diagnostic = Diagnostic.Create(RuleByScope, member.GetLocation(), first, second);
            context.ReportDiagnostic(diagnostic);
        }

        private static void ReportDiagnosticType(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax member, string first, string second)
        {
            var diagnostic = Diagnostic.Create(RuleByType, member.GetLocation(), first, second);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
