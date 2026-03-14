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

        TypedConstant? secondMetaCommandEvaluatorConstructorArgument = metaCommandEvaluatorAttribute?
            .ConstructorArguments.Skip(1).FirstOrDefault();
        string? infoMethodName = secondMetaCommandEvaluatorConstructorArgument?.IsNull ?? true ? null : secondMetaCommandEvaluatorConstructorArgument?.Value as string;

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

        return new MetaCommandEvaluatorModel(containingNamespace, evaluatorTypeName, typeAccessibility, isTypeAbstract, isTypeSealed, handlerMethodName, handlerMethodAccessibility, isHandlerMethodVirtual, isHandlerMethodOverride, infoMethodName, metaCommandCandidates, duplicateNames, attributeLocation);
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
        Dictionary<string, SpecialType> parameters = methodSymbol.Parameters.ToDictionary(parameter => parameter.Name, parameter => parameter.Type.SpecialType);

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

        return new MetaCommandModel(metaCommandName, metaCommandDescription, metaCommandAliases, methodName, parameters, attributeLocation);
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

                if (metaCommand.Parameters.Any(parameter => parameter.Value != SpecialType.System_String))
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

            int paramCount = metaCommand.Parameters.Count;

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
            // /help dispatch (only in the root handler, not in overrides — they fall through to base)
            if (metaCommandEvaluatorModel.InfoMethodName is not null)
            {
                sb.AppendLine(indent + "if (string.Equals(commandName, \"help\", global::System.StringComparison.OrdinalIgnoreCase))");
                sb.AppendLine(indent + "{");
                sb.AppendLine(indent + "    if (args.Count == 0)");
                sb.AppendLine(indent + "    {");
                sb.AppendLine(indent + "        var orderedMetaCommandInfos = global::System.Linq.Enumerable.OrderBy(" + metaCommandEvaluatorModel.InfoMethodName + "(), info => info.Name, global::System.StringComparer.OrdinalIgnoreCase);");
                sb.AppendLine(indent + "        var metaCommandInfos = global::System.Linq.Enumerable.ToList(orderedMetaCommandInfos);");
                sb.AppendLine(indent + "        var prefixes = new global::System.Collections.Generic.List<string>(metaCommandInfos.Count);");
                sb.AppendLine(indent + "        int maxLen = 0;");
                sb.AppendLine(indent + "        foreach (var info in metaCommandInfos)");
                sb.AppendLine(indent + "        {");
                sb.AppendLine(indent + "            string aliasText = info.Aliases.Length > 0");
                sb.AppendLine(indent + "                ? \" (aliases: \" + string.Join(\", \", global::System.Linq.Enumerable.Select(info.Aliases, a => \"/\" + a)) + \")\"");
                sb.AppendLine(indent + "                : \"\";");
                sb.AppendLine(indent + "            string parametersText = info.Parameters.Length > 0");
                sb.AppendLine(indent + "                ? \" \" + string.Join(\" \", global::System.Linq.Enumerable.Select(info.Parameters, p => \"<\" + p + \">\"))");
                sb.AppendLine(indent + "                : \"\";");
                sb.AppendLine(indent + "            string prefix = \"/\" + info.Name + parametersText + aliasText;");
                sb.AppendLine(indent + "            prefixes.Add(prefix);");
                sb.AppendLine(indent + "            if (prefix.Length > maxLen) maxLen = prefix.Length;");
                sb.AppendLine(indent + "        }");
                sb.AppendLine(indent + "        for (int i = 0; i < metaCommandInfos.Count; i++)");
                sb.AppendLine(indent + "        {");
                sb.AppendLine(indent + "            global::System.Console.WriteLine(prefixes[i].PadRight(maxLen + 3) + metaCommandInfos[i].Description);");
                sb.AppendLine(indent + "        }");
                sb.AppendLine(indent + "        return;");
                sb.AppendLine(indent + "    }");
                sb.AppendLine(indent + "    global::System.Console.ForegroundColor = global::System.ConsoleColor.DarkRed;");
                sb.AppendLine(indent + "    global::System.Console.Error.WriteLine(\"Command 'help' expects 0 argument(s), but got \" + args.Count + \".\");");
                sb.AppendLine(indent + "    global::System.Console.ResetColor();");
                sb.AppendLine(indent + "    return;");
                sb.AppendLine(indent + "}");
                sb.AppendLine();
            }

            sb.AppendLine(indent + "global::System.Console.ForegroundColor = global::System.ConsoleColor.DarkRed;");
            sb.AppendLine(indent + "global::System.Console.Error.WriteLine($\"Unknown command '{input}'.\");");
            sb.AppendLine(indent + "global::System.Console.ResetColor();");
        }

        sb.AppendLine("    }");

        // Info method — emits metadata about all meta commands for /help
        if (metaCommandEvaluatorModel.InfoMethodName is not null)
        {
            sb.AppendLine();
            sb.Append("    protected");
            if (metaCommandEvaluatorModel.IsHandlerMethodOverride)
            {
                sb.Append(" override");
            }
            else
            {
                sb.Append(" virtual");
            }
            sb.AppendLine(" global::System.Collections.Generic.IEnumerable<(string Name, string[] Parameters, string Description, string[] Aliases)> " + metaCommandEvaluatorModel.InfoMethodName + "()");
            sb.AppendLine("    {");

            if (metaCommandEvaluatorModel.IsHandlerMethodOverride)
            {
                // Override: concat own commands onto base
                sb.Append(indent + "return global::System.Linq.Enumerable.Concat(base." + metaCommandEvaluatorModel.InfoMethodName + "(), ");
                sb.AppendLine("new (string Name, string[] Parameters, string Description, string[] Aliases)[]");
                sb.AppendLine(indent + "{");
            }
            else
            {
                // Root: return own commands + hardcoded help entry
                sb.AppendLine(indent + "return new (string Name, string[] Parameters, string Description, string[] Aliases)[]");
                sb.AppendLine(indent + "{");
                sb.AppendLine(indent + "    (\"help\", new string[] { }, \"Lists all available meta commands.\", new string[] { }),");
            }

            foreach (MetaCommandModel metaCommand in metaCommandEvaluatorModel.MetaCommands)
            {
                StringBuilder aliasArray = new();
                aliasArray.Append("new string[] { ");
                bool firstAlias = true;
                foreach (string alias in metaCommand.Aliases)
                {
                    if (!firstAlias)
                    {
                        aliasArray.Append(", ");
                    }

                    aliasArray.Append("\"" + alias + "\"");
                    firstAlias = false;
                }
                aliasArray.Append(" }");

                StringBuilder parametersArray = new();
                parametersArray.Append("new string[] { ");
                bool firstParameter = true;
                foreach (string parameter in metaCommand.Parameters.Keys)
                {
                    if (!firstParameter)
                    {
                        parametersArray.Append(", ");
                    }

                    parametersArray.Append("\"" + parameter + "\"");
                    firstParameter = false;
                }
                parametersArray.Append(" }");

                sb.AppendLine(indent + "    (\"" + metaCommand.Name + "\", " + parametersArray + ", \"" + metaCommand.Description + "\", " + aliasArray + "),");
            }

            if (metaCommandEvaluatorModel.IsHandlerMethodOverride)
            {
                sb.AppendLine(indent + "});");
            }
            else
            {
                sb.AppendLine(indent + "};");
            }

            sb.AppendLine("    }");
        }

        sb.AppendLine("}");

        context.AddSource(metaCommandEvaluatorModel.EvaluatorTypeName + ".g.cs", sb.ToString());
    }
}
