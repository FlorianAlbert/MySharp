namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundLabel
{
    public BoundLabel(string name)
    {
        Name = name;
    }

    public string Name { get; }

    override public string ToString() => Name;
}
