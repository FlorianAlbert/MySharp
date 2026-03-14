using Microsoft.CodeAnalysis;

namespace FlorianAlbert.MySharp.Interpreter.Generators;

internal class MetaCommandModel
{
    public MetaCommandModel(string? name,
        string? description,
        IEnumerable<string> aliases,
        string methodName,
        Dictionary<string, SpecialType> parameters,
        Location attributeLocation)
    {
        Name = name;
        Description = description;
        Aliases = aliases;
        MethodName = methodName;
        Parameters = parameters;
        AttributeLocation = attributeLocation;
    }

    public string? Name { get; }

    public string? Description { get; }

    public IEnumerable<string> Aliases { get; }

    public string MethodName { get; }

    public Dictionary<string, SpecialType> Parameters { get; }

    public Location AttributeLocation { get; }

    public bool HasErrors => string.IsNullOrEmpty(Name) || 
        string.IsNullOrEmpty(Description) ||
        Parameters.Any(parameter => parameter.Value is not SpecialType.System_String);
}
