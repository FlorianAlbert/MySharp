using FlorianAlbert.MySharp.Interpreter.Annotations;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace FlorianAlbert.MySharp.Interpreter.Generators;

[Generator]
public class MetaCommandEvaluatorGenerator : IIncrementalGenerator
{
    private static string _MetaCommandEvaluatorAttributeFullName => typeof(MetaCommandEvaluatorAttribute).FullName;
    private static string _MetaCommandAttributeFullName => typeof(MetaCommandAttribute).FullName;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<MetaCommandEvaluatorModel> incrementalValuesProvider =
            context.SyntaxProvider.ForAttributeWithMetadataName(_MetaCommandEvaluatorAttributeFullName,
                                                                FilterSyntaxNodes,
                                                                TransformMetaCommandEvaluatorAttribute)
                                  .Where(m => m is not null)!;

        context.RegisterSourceOutput(incrementalValuesProvider, Emit);
    }

    private bool FilterSyntaxNodes(SyntaxNode node, CancellationToken _)
    {
        if (!node.IsKind(SyntaxKind.ClassDeclaration))
        {
            return false;
        }

        ClassDeclarationSyntax classDeclarationSyntax = (ClassDeclarationSyntax) node;

        return classDeclarationSyntax.Modifiers.Any(SyntaxKind.PartialKeyword);
    }

    private MetaCommandEvaluatorModel? TransformMetaCommandEvaluatorAttribute(GeneratorAttributeSyntaxContext context, CancellationToken token)
    {
        if (context.TargetSymbol is not INamedTypeSymbol typeSymbol)
        {
            return null;
        }

        string? containingNamespace = typeSymbol.ContainingNamespace?.ToDisplayString();
        string evaluatorTypeName = typeSymbol.Name;
        string typeAccessibility = AccessibilityToKeyword(typeSymbol.DeclaredAccessibility);
        bool isTypeAbstract = typeSymbol.IsAbstract;
        bool isTypeSealed = typeSymbol.IsSealed;

        AttributeData? metaCommandEvaluatorAttribute = context.Attributes.FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == _MetaCommandEvaluatorAttributeFullName);
        Location attributeLocation = metaCommandEvaluatorAttribute?.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None;

        TypedConstant? firstMetaCommandEvaluatorConstructorArgument = metaCommandEvaluatorAttribute?
            .ConstructorArguments.FirstOrDefault();
        string? handlerMethodName = firstMetaCommandEvaluatorConstructorArgument?.IsNull ?? true ? null : firstMetaCommandEvaluatorConstructorArgument?.Value as string;

        IEnumerable<IMethodSymbol> typeMethods = typeSymbol.GetMembers()
            .Where(member => member.Kind is SymbolKind.Method)
            .Cast<IMethodSymbol>();

        string handlerMethodAccessibility = "public";
        bool isHandlerMethodVirtual = false;
        bool isHandlerMethodOverride = false;

        if (handlerMethodName is not null)
        {
            IMethodSymbol? handlerMethodSymbol = typeMethods.SingleOrDefault(methodSymbol => IsPartialHandlerMethodSignature(methodSymbol, handlerMethodName));
            handlerMethodName = handlerMethodSymbol?.Name;

            if (handlerMethodSymbol is not null)
            {
                handlerMethodAccessibility = AccessibilityToKeyword(handlerMethodSymbol.DeclaredAccessibility);
                isHandlerMethodVirtual = handlerMethodSymbol.IsVirtual;
                isHandlerMethodOverride = handlerMethodSymbol.IsOverride;
            }
        }

        List<MetaCommandModel> metaCommandCandidates = [.. typeMethods.Where(IsMetaCommandMethodSignature).Select(CreateMetaCommandModelFromMethodSymbol)];

        // Detect duplicate command names/aliases (case-insensitive)
        List<string> allNames = [];
        foreach (MetaCommandModel cmd in metaCommandCandidates)
        {
            if (!string.IsNullOrEmpty(cmd.Name))
            {
                allNames.Add(cmd.Name!);
            }
            foreach (string alias in cmd.Aliases)
            {
                allNames.Add(alias);
            }
        }

        IEnumerable<string> duplicateNames = allNames
            .GroupBy(n => n, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        return new MetaCommandEvaluatorModel(containingNamespace, evaluatorTypeName, typeAccessibility, isTypeAbstract, isTypeSealed, handlerMethodName, handlerMethodAccessibility, isHandlerMethodVirtual, isHandlerMethodOverride, metaCommandCandidates, duplicateNames, attributeLocation);
    }

    private static string AccessibilityToKeyword(Accessibility accessibility)
    {
        switch (accessibility)
        {
            case Accessibility.Public: return "public";
            case Accessibility.Internal: return "internal";
            case Accessibility.Protected: return "protected";
            case Accessibility.ProtectedOrInternal: return "protected internal";
            case Accessibility.ProtectedAndInternal: return "private protected";
            case Accessibility.Private: return "private";
            default: return "internal";
        }
    }

    private MetaCommandModel CreateMetaCommandModelFromMethodSymbol(IMethodSymbol methodSymbol)
    {
        string methodName = methodSymbol.Name;
        IEnumerable<SpecialType> parameterTypes = methodSymbol.Parameters.Select(parameter => parameter.Type.SpecialType);

        AttributeData? metaCommandAttribute = methodSymbol.GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.ToDisplayString() == _MetaCommandAttributeFullName);
        Location attributeLocation = metaCommandAttribute?.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None;

        string? metaCommandName = null;
        string? metaCommandDescription = null;
        IEnumerable<string> metaCommandAliases = [];
        if (metaCommandAttribute is not null)
        {
            TypedConstant? firstMetaCommandConstructorArgument = metaCommandAttribute
                .ConstructorArguments.FirstOrDefault();
            metaCommandName = firstMetaCommandConstructorArgument?.IsNull ?? true ? null : firstMetaCommandConstructorArgument?.Value as string;

            TypedConstant? secondMetaCommandConstructorArgument = metaCommandAttribute
                .ConstructorArguments.Skip(1).FirstOrDefault();
            metaCommandDescription = secondMetaCommandConstructorArgument?.IsNull ?? true ? null : secondMetaCommandConstructorArgument?.Value as string;

            KeyValuePair<string, TypedConstant> aliasesMetaCommandConstructorArgument = metaCommandAttribute
                .NamedArguments.FirstOrDefault(kv => kv.Key == nameof(MetaCommandAttribute.Aliases));
            if (!aliasesMetaCommandConstructorArgument.Equals(default(KeyValuePair<string, TypedConstant>)))
            {
                metaCommandAliases = aliasesMetaCommandConstructorArgument.Value.Values.Select(v => v.Value as string).Where(alias => alias is not null)!;
            }
        }

        return new MetaCommandModel(metaCommandName, metaCommandDescription, metaCommandAliases, methodName, parameterTypes, attributeLocation);
    }

    private static bool IsPartialHandlerMethodSignature(IMethodSymbol methodSymbol, string expectedHandlerMethodName)
    {
        bool methodNameMatches = methodSymbol.Name == expectedHandlerMethodName;
        bool isPartial = methodSymbol.IsPartialDefinition;
        bool hasCorrectParameters = methodSymbol.Parameters.Length == 1 && methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_String;

        return methodNameMatches && isPartial && hasCorrectParameters;
    }

    private static bool IsMetaCommandMethodSignature(IMethodSymbol methodSymbol)
    {
        bool hasMetaCommandAttribute = methodSymbol.GetAttributes().Any(attr => attr.AttributeClass?.ToDisplayString() == _MetaCommandAttributeFullName);
        return hasMetaCommandAttribute;
    }

    private static void Emit(SourceProductionContext context, MetaCommandEvaluatorModel metaCommandEvaluatorModel)
    {
        if (metaCommandEvaluatorModel.HasErrors)
        {
            if (metaCommandEvaluatorModel.EvaluatorTypeNamespace is null)
            {
                context.ReportMissingNamespace(metaCommandEvaluatorModel.AttributeLocation, metaCommandEvaluatorModel.EvaluatorTypeName);
            }

            if (metaCommandEvaluatorModel.HandlerMethod is null)
            {
                context.ReportNoMatchingHandlerMethod(metaCommandEvaluatorModel.AttributeLocation, metaCommandEvaluatorModel.EvaluatorTypeName);
            }

            foreach (string duplicateName in metaCommandEvaluatorModel.DuplicateCommandNames)
            {
                context.ReportDuplicateCommandNameOrAlias(metaCommandEvaluatorModel.AttributeLocation, duplicateName, metaCommandEvaluatorModel.EvaluatorTypeName);
            }

            foreach (MetaCommandModel metaCommand in metaCommandEvaluatorModel.MetaCommands)
            {
                if (string.IsNullOrEmpty(metaCommand.Name))
                {
                    context.ReportMissingMetaCommandName(metaCommand.AttributeLocation, metaCommand.MethodName, metaCommandEvaluatorModel.EvaluatorTypeName);
                }

                if (string.IsNullOrEmpty(metaCommand.Description))
                {
                    context.ReportMissingMetaCommandDescription(metaCommand.AttributeLocation, metaCommand.MethodName, metaCommandEvaluatorModel.EvaluatorTypeName);
                }

                if (metaCommand.ParameterTypes.Any(type => type != SpecialType.System_String))
                {
                    context.ReportInvalidMetaCommandParameterTypes(metaCommand.AttributeLocation, metaCommand.MethodName, metaCommandEvaluatorModel.EvaluatorTypeName);
                }
            }

            return;
        }

        StringBuilder sb = new();
        string indent = "        ";

        sb.AppendLine("/* Generated code */");
        sb.AppendLine("namespace " + metaCommandEvaluatorModel.EvaluatorTypeNamespace + ";");
        sb.AppendLine();

        // Class declaration
        sb.Append(metaCommandEvaluatorModel.TypeAccessibility);
        if (metaCommandEvaluatorModel.IsTypeAbstract)
        {
            sb.Append(" abstract");
        }

        if (metaCommandEvaluatorModel.IsTypeSealed)
        {
            sb.Append(" sealed");
        }

        sb.AppendLine(" partial class " + metaCommandEvaluatorModel.EvaluatorTypeName);
        sb.AppendLine("{");

        // Handler method declaration
        sb.Append("    " + metaCommandEvaluatorModel.HandlerMethodAccessibility);
        if (metaCommandEvaluatorModel.IsHandlerMethodOverride)
        {
            sb.Append(" override");
        }
        else if (metaCommandEvaluatorModel.IsHandlerMethodVirtual)
        {
            sb.Append(" virtual");
        }

        sb.AppendLine(" partial void " + metaCommandEvaluatorModel.HandlerMethod + "(string input)");
        sb.AppendLine("    {");

        // Inline lexer
        sb.AppendLine(indent + "int pos = 0;");
        sb.AppendLine(indent + "if (pos < input.Length && input[pos] == '/') pos++;");
        sb.AppendLine(indent + "int start = pos;");
        sb.AppendLine(indent + "while (pos < input.Length && input[pos] != ' ') pos++;");
        sb.AppendLine(indent + "string commandName = input.Substring(start, pos - start);");
        sb.AppendLine(indent + "var args = new global::System.Collections.Generic.List<string>();");
        sb.AppendLine(indent + "while (pos < input.Length)");
        sb.AppendLine(indent + "{");
        sb.AppendLine(indent + "    while (pos < input.Length && input[pos] == ' ') pos++;");
        sb.AppendLine(indent + "    if (pos >= input.Length) break;");
        sb.AppendLine(indent + "    if (input[pos] == '\"')");
        sb.AppendLine(indent + "    {");
        sb.AppendLine(indent + "        pos++;");
        sb.AppendLine(indent + "        start = pos;");
        sb.AppendLine(indent + "        while (pos < input.Length && input[pos] != '\"') pos++;");
        sb.AppendLine(indent + "        args.Add(input.Substring(start, pos - start));");
        sb.AppendLine(indent + "        if (pos < input.Length) pos++;");
        sb.AppendLine(indent + "    }");
        sb.AppendLine(indent + "    else");
        sb.AppendLine(indent + "    {");
        sb.AppendLine(indent + "        start = pos;");
        sb.AppendLine(indent + "        while (pos < input.Length && input[pos] != ' ') pos++;");
        sb.AppendLine(indent + "        args.Add(input.Substring(start, pos - start));");
        sb.AppendLine(indent + "    }");
        sb.AppendLine(indent + "}");
        sb.AppendLine();

        // Command dispatch
        foreach (MetaCommandModel metaCommand in metaCommandEvaluatorModel.MetaCommands)
        {
            // Build condition: name || alias1 || alias2 ...
            StringBuilder condition = new();
            condition.Append("string.Equals(commandName, \"" + metaCommand.Name + "\", global::System.StringComparison.OrdinalIgnoreCase)");
            foreach (string alias in metaCommand.Aliases)
            {
                condition.Append("\n" + indent + "    || string.Equals(commandName, \"" + alias + "\", global::System.StringComparison.OrdinalIgnoreCase)");
            }

            int paramCount = metaCommand.ParameterTypes.Count();

            sb.AppendLine(indent + "if (" + condition + ")");
            sb.AppendLine(indent + "{");

            // Arg count check
            sb.AppendLine(indent + "    if (args.Count == " + paramCount + ")");
            sb.AppendLine(indent + "    {");

            // Build method call with args
            StringBuilder methodCall = new();
            methodCall.Append(metaCommand.MethodName + "(");
            for (int i = 0; i < paramCount; i++)
            {
                if (i > 0)
                {
                    methodCall.Append(", ");
                }

                methodCall.Append("args[" + i + "]");
            }
            methodCall.Append(")");

            sb.AppendLine(indent + "        " + methodCall + ";");
            sb.AppendLine(indent + "        return;");
            sb.AppendLine(indent + "    }");

            // Arg count mismatch error
            sb.AppendLine(indent + "    global::System.Console.ForegroundColor = global::System.ConsoleColor.DarkRed;");
            sb.AppendLine(indent + "    global::System.Console.Error.WriteLine($\"Command '" + metaCommand.Name + "' expects " + paramCount + " argument(s), but got {args.Count}.\");");
            sb.AppendLine(indent + "    global::System.Console.ResetColor();");
            sb.AppendLine(indent + "    return;");

            sb.AppendLine(indent + "}");
            sb.AppendLine();
        }

        // No command matched
        if (metaCommandEvaluatorModel.IsHandlerMethodOverride)
        {
            sb.AppendLine(indent + "base." + metaCommandEvaluatorModel.HandlerMethod + "(input);");
        }
        else
        {
            sb.AppendLine(indent + "global::System.Console.ForegroundColor = global::System.ConsoleColor.DarkRed;");
            sb.AppendLine(indent + "global::System.Console.Error.WriteLine($\"Unknown command '{input}'.\");");
            sb.AppendLine(indent + "global::System.Console.ResetColor();");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource(metaCommandEvaluatorModel.EvaluatorTypeName + ".g.cs", sb.ToString());
    }
}
