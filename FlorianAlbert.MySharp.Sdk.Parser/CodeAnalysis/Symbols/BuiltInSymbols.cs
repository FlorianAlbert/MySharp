using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

public abstract class BuiltInSymbols<TSymbol>
     where TSymbol : Symbol
{
    public abstract ImmutableHashSet<TSymbol> GetAll();
}
