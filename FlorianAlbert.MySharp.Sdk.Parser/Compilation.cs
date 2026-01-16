using FlorianAlbert.MySharp.Sdk.Parser.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser;

public sealed class Compilation
{
    public Compilation(SyntaxTree syntaxTree)
    {
        SyntaxTree = syntaxTree;
    }

    public SyntaxTree SyntaxTree { get; }

    public EvaluationResult Evaluate(Dictionary<VariableSymbol, object?> variables)
    {
        Binder binder = new(variables);
        BoundExpression boundExpression = binder.BindExpression(SyntaxTree.Root);

        DiagnosticBag diagnostics = [.. SyntaxTree.Diagnostics, .. binder.Diagnostics];
        if (diagnostics.Count > 0)
        {
            return new EvaluationResult(diagnostics, null);
        }

        Evaluator evaluator = new(boundExpression, variables);
        object? result = evaluator.Evaluate();

        return new EvaluationResult([], result);
    }
}
