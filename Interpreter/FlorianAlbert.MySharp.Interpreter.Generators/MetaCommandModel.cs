using Microsoft.CodeAnalysis;

namespace FlorianAlbert.MySharp.Interpreter.Generators;

internal class MetaCommandModel
{
    public MetaCommandModel(string? name,
        string? description,
        IEnumerable<string> aliases,
        string methodName,
        IEnumerable<SpecialType> parameterTypes,
        Location attributeLocation)
    {
        Name = name;
        Description = description;
        Aliases = aliases;
        MethodName = methodName;
        ParameterTypes = parameterTypes;
        AttributeLocation = attributeLocation;
    }

    public string? Name { get; }

    public string? Description { get; }

    public IEnumerable<string> Aliases { get; }

    public string MethodName { get; }

    public IEnumerable<SpecialType> ParameterTypes { get; }

    public Location AttributeLocation { get; }

    public bool HasErrors => string.IsNullOrEmpty(Name) || 
        string.IsNullOrEmpty(Description) ||
        ParameterTypes.Any(type => type is not SpecialType.System_String);
}
