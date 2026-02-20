
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundCompilationUnit
{
    public BoundCompilationUnit(BoundCompilationUnit? previous, ImmutableArray<Diagnostic> diagnostics, BoundGlobalScope globalScope, BoundProgram program)
    {
        Previous = previous;
        Diagnostics = diagnostics;
        GlobalScope = globalScope;
        Program = program;
    }

    public BoundCompilationUnit? Previous { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public BoundGlobalScope GlobalScope { get; }

    public BoundProgram Program { get; }
}
