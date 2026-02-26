using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope(ImmutableArray<VariableSymbol> variableSymbols,
        ImmutableArray<FunctionSymbol> functionSymbols,
        BoundBlockStatement boundStatement)
    {
        Variables = variableSymbols;
        Functions = functionSymbols;
        Statement = boundStatement;
    }

    public ImmutableArray<VariableSymbol> Variables { get; }

    public ImmutableArray<FunctionSymbol> Functions { get; }

    public BoundBlockStatement Statement { get; }
}
