namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

public sealed class TypeSymbol : Symbol
{
    private TypeSymbol(string name) : base(name)
    {
    }

    public override SymbolKind Kind => SymbolKind.Type;

    public static readonly TypeSymbol Error = new("<error>");

    public static readonly TypeSymbol Bool = new("bool");

    public static readonly TypeSymbol Int32 = new("int32");

    public static readonly TypeSymbol String = new("string");

    public static readonly TypeSymbol Character = new("char");
}
