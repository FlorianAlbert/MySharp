using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Evaluation;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Lowering;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.Extensions;
using System.Collections.Immutable;

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

    internal BoundCompilationUnit CompilationUnit
    {
        get
        {
            if (field is null)
            {
                BoundCompilationUnit compilationUnit = Binder.BindCompilationUnit(Previous?.CompilationUnit, SyntaxTree.Root);
                Interlocked.CompareExchange(ref field, compilationUnit, null);
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
        DiagnosticBag diagnostics = [.. SyntaxTree.Diagnostics, .. CompilationUnit.Diagnostics];
        if (diagnostics.Count > 0)
        {
            return new EvaluationResult([.. diagnostics], null);
        }

        BoundBlockStatement blockStatement = GetStatement();
        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies = GetFunctionBodies();
        Evaluator evaluator = new(blockStatement, functionBodies, variables);
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
        BoundStatement statement = CompilationUnit.GlobalScope.Statement;
        return Lowerer.Lower(statement);
    }

    private ImmutableDictionary<FunctionSymbol, BoundBlockStatement> GetFunctionBodies()
    {
        ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Builder loweredFunctionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();
        foreach ((FunctionSymbol functionSymbol, BoundBlockStatement functionBody) in CompilationUnit.Program.FunctionBodies)
        {
            BoundBlockStatement loweredFunctionBody = Lowerer.Lower(functionBody);
            loweredFunctionBodies.Add(functionSymbol, loweredFunctionBody);
        }

        return loweredFunctionBodies.ToImmutable();
    }
}
