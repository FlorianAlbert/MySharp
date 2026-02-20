using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundProgram
{
    public BoundProgram(ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies)
    {
        FunctionBodies = functionBodies;
    }

    public ImmutableDictionary<FunctionSymbol, BoundBlockStatement> FunctionBodies { get; }
}