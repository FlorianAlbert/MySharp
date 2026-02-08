using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundGlobalScope
{
    public BoundGlobalScope(BoundGlobalScope? previous, ImmutableArray<Diagnostic> diagnostics, ImmutableArray<VariableSymbol> variableSymbols, BoundStatement boundStatement)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        Variables = variableSymbols;
        Statement = boundStatement;
    }

    public BoundGlobalScope? Previous { get; }
    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public ImmutableArray<VariableSymbol> Variables { get; }
    public BoundStatement Statement { get; }
}
