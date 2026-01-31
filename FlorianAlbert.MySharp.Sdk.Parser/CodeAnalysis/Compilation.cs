using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Lowering;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

public sealed class Compilation
{
    public Compilation(SyntaxTree syntaxTree)
        : this(null, syntaxTree)
    {
    }

    private Compilation(Compilation? previous, SyntaxTree syntaxTree)
    {
        Previous = previous;
        SyntaxTree = syntaxTree;
    }

    public Compilation? Previous { get; }
    public SyntaxTree SyntaxTree { get; }

    internal BoundGlobalScope GlobalScope
    {
        get
        {
            if (field is null)
            {
                BoundGlobalScope globalScope = Binder.BindGlobalScope(Previous?.GlobalScope, SyntaxTree.Root);
                Interlocked.CompareExchange(ref field, globalScope, null);
            }

            return field;
        }
    }

    public Compilation ContinueWith(SyntaxTree syntaxTree)
    {
        return new Compilation(this, syntaxTree);
    }

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object?> variables)
    {
        DiagnosticBag diagnostics = [.. SyntaxTree.Diagnostics, .. GlobalScope.Diagnostics];
        if (diagnostics.Count > 0)
        {
            return new EvaluationResult([.. diagnostics], null);
        }

        BoundBlockStatement blockStatement = GetStatement();
        Evaluator evaluator = new(blockStatement, variables);
        object? result = evaluator.Evaluate();

        return new EvaluationResult([], result);
    }

    public void EmitTree(TextWriter writer)
    {
        BoundBlockStatement blockStatement = GetStatement();
        blockStatement.WriteTo(writer);
    }

    private BoundBlockStatement GetStatement()
    {
        BoundStatement statement = GlobalScope.Statement;
        return Lowerer.Lower(statement);
    }
}
