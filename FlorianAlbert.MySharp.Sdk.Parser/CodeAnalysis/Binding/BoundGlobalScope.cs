using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope(BoundGlobalScope? previous, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<VariableSymbol> variableSymbols, BoundExpression boundExpression)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        Variables = variableSymbols;
        Expression = boundExpression;
    }

    public BoundGlobalScope? Previous { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public ImmutableArray<VariableSymbol> Variables { get; }
    public BoundExpression Expression { get; }
}
