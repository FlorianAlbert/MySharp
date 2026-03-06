using Microsoft.CodeAnalysis;

namespace FlorianAlbert.MySharp.Interpreter.Generators;

internal static class SourceProductionContextExtensions
{
    extension(SourceProductionContext context)
    {
        public void ReportMissingNamespace(Location errorLocation, string? typeName)
        {
            Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor(
                            id: "MCEG001",
                            title: "MetaCommandEvaluator is not declared within a namespace",
                            messageFormat: "The class '{0}' is marked with MetaCommandEvaluatorAttribute but is not declared within a namespace.",
                            category: "MetaCommandEvaluatorGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                            errorLocation,
                            typeName);

            context.ReportDiagnostic(diagnostic);
        }

        public void ReportNoMatchingHandlerMethod(Location errorLocation, string? typeName)
        {
            Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor(
                            id: "MCEG002",
                            title: "No matching handler method found",
                            messageFormat: "The class '{0}' is marked with MetaCommandEvaluatorAttribute, but no matching method taking a single string parameter was found.",
                            category: "MetaCommandEvaluatorGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                            errorLocation,
                            typeName);

            context.ReportDiagnostic(diagnostic);
        }

        public void ReportMissingMetaCommandName(Location errorLocation, string methodName, string typeName)
        {
            Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor(
                            id: "MCEG003",
                            title: "MetaCommand is missing a name",
                            messageFormat: "The method '{0}' in class '{1}' is marked with MetaCommandAttribute but does not have a valid name.",
                            category: "MetaCommandEvaluatorGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                            errorLocation,
                            methodName,
                            typeName);

            context.ReportDiagnostic(diagnostic);
        }

        public void ReportMissingMetaCommandDescription(Location errorLocation, string methodName, string typeName)
        {
            Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor(
                            id: "MCEG004",
                            title: "MetaCommand is missing a description",
                            messageFormat: "The method '{0}' in class '{1}' is marked with MetaCommandAttribute but does not have a valid description.",
                            category: "MetaCommandEvaluatorGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                            errorLocation,
                            methodName,
                            typeName);

            context.ReportDiagnostic(diagnostic);
        }

        public void ReportInvalidMetaCommandParameterTypes(Location errorLocation, string methodName, string typeName)
        {
            Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor(
                            id: "MCEG005",
                            title: "MetaCommand has invalid parameter types",
                            messageFormat: "The method '{0}' in class '{1}' is marked with MetaCommandAttribute but has invalid parameter types. Only string parameters are supported.",
                            category: "MetaCommandEvaluatorGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                            errorLocation,
                            methodName,
                            typeName);

            context.ReportDiagnostic(diagnostic);
        }

        public void ReportDuplicateCommandNameOrAlias(Location errorLocation, string duplicateName, string typeName)
        {
            Diagnostic diagnostic = Diagnostic.Create(new DiagnosticDescriptor(
                            id: "MCEG006",
                            title: "Duplicate meta command name or alias",
                            messageFormat: "The command name or alias '{0}' is registered more than once in class '{1}'.",
                            category: "MetaCommandEvaluatorGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                            errorLocation,
                            duplicateName,
                            typeName);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
