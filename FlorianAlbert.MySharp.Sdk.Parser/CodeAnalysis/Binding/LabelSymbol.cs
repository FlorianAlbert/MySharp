namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class LabelSymbol
{
    public LabelSymbol(string name)
    {
        Name = name;
    }

    public string Name { get; }

    override public string ToString() => Name;
}
