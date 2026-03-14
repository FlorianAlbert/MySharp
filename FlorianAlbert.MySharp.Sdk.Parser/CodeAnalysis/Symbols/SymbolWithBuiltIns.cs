namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

public abstract class SymbolWithBuiltIns<TSymbol, TBuiltInSymbols> : Symbol
    where TSymbol : Symbol
    where TBuiltInSymbols : BuiltInSymbols<TSymbol>, new()
{
    private protected SymbolWithBuiltIns(string name) : base(name)
    {
    }

    public static new TBuiltInSymbols BuiltIns { get; } = new();
}
