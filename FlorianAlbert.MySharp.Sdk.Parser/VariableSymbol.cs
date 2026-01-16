namespace FlorianAlbert.MySharp.Sdk.Parser;

public sealed class VariableSymbol
{
    public VariableSymbol(string name, Type type)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }

    public Type Type { get; }
}
