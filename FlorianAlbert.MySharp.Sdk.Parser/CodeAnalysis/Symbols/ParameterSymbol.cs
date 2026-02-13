namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

public class ParameterSymbol : VariableSymbol
{
    internal ParameterSymbol(string name, TypeSymbol type)
        : base(name, isReadOnly: true, type)
    {
    }

    public override SymbolKind Kind => SymbolKind.Parameter;
}
