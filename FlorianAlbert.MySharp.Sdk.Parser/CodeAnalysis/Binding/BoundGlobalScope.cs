using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope(ImmutableHashSet<VariableSymbol> variableSymbols,
        ImmutableHashSet<FunctionSymbol> functionSymbols,
        BoundBlockStatement boundStatement)
    {
        Symbols = [.. variableSymbols, .. functionSymbols];
        Variables = variableSymbols;
        Functions = functionSymbols;
        Statement = boundStatement;
    }

    public ImmutableHashSet<Symbol> Symbols { get; }

    public ImmutableHashSet<VariableSymbol> Variables { get; }

    public ImmutableHashSet<FunctionSymbol> Functions { get; }

    public BoundBlockStatement Statement { get; }
}
