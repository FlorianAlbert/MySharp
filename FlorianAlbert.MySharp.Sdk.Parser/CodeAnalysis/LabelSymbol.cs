namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

internal sealed class LabelSymbol
{
    public LabelSymbol(string name)
    {
        Name = name;
    }

    public string Name { get; }

    override public string ToString() => Name;
}
