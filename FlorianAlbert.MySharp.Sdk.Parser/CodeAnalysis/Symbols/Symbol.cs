using FlorianAlbert.MySharp.Sdk.Parser.Extensions;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

public abstract class Symbol
{
    private protected Symbol(string name)
    {
        Name = name;
    }

    public abstract SymbolKind Kind { get; }

    public string Name { get; }

    public override string ToString()
    {
        using StringWriter stringWriter = new();
        this.WriteTo(stringWriter);
        return stringWriter.ToString();
    }
}
