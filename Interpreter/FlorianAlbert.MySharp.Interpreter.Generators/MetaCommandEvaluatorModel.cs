using Microsoft.CodeAnalysis;

namespace FlorianAlbert.MySharp.Interpreter.Generators;

internal class MetaCommandEvaluatorModel
{
    public MetaCommandEvaluatorModel(string? evaluatorTypeNamespace,
        string evaluatorTypeName,
        string typeAccessibility,
        bool isTypeAbstract,
        bool isTypeSealed,
        string? handlerMethod,
        string handlerMethodAccessibility,
        bool isHandlerMethodVirtual,
        bool isHandlerMethodOverride,
        IEnumerable<MetaCommandModel> metaCommands,
        IEnumerable<string> duplicateCommandNames,
        Location attributeLocation)
    {
        EvaluatorTypeNamespace = evaluatorTypeNamespace;
        EvaluatorTypeName = evaluatorTypeName;
        TypeAccessibility = typeAccessibility;
        IsTypeAbstract = isTypeAbstract;
        IsTypeSealed = isTypeSealed;
        HandlerMethod = handlerMethod;
        HandlerMethodAccessibility = handlerMethodAccessibility;
        IsHandlerMethodVirtual = isHandlerMethodVirtual;
        IsHandlerMethodOverride = isHandlerMethodOverride;
        MetaCommands = metaCommands;
        DuplicateCommandNames = duplicateCommandNames;
        AttributeLocation = attributeLocation;
    }

    public string? EvaluatorTypeNamespace { get; }

    public string EvaluatorTypeName { get; }

    public string TypeAccessibility { get; }

    public bool IsTypeAbstract { get; }

    public bool IsTypeSealed { get; }

    public string? HandlerMethod { get; }

    public string HandlerMethodAccessibility { get; }

    public bool IsHandlerMethodVirtual { get; }

    public bool IsHandlerMethodOverride { get; }

    public IEnumerable<MetaCommandModel> MetaCommands { get; }

    public IEnumerable<string> DuplicateCommandNames { get; }

    public Location AttributeLocation { get; }

    public bool HasErrors => EvaluatorTypeNamespace is null ||
        HandlerMethod is null ||
        DuplicateCommandNames.Any() ||
        MetaCommands.Any(metaCommand => metaCommand.HasErrors);
}