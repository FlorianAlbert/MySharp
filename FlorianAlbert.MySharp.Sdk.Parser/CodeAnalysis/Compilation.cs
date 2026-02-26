using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Evaluation;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Lowering;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.Extensions;
using System.CodeDom.Compiler;
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

        BoundBlockStatement blockStatement = CompilationUnit.GlobalScope.Statement;
        ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies = CompilationUnit.Program.FunctionBodies;
        Evaluator evaluator = new(blockStatement, functionBodies, variables);
        object? result = evaluator.Evaluate();

        return new EvaluationResult([], result);
    }

    public void EmitTree(TextWriter writer)
    {
        IndentedTextWriter indentedTextWriter = writer as IndentedTextWriter ?? new IndentedTextWriter(writer);

        if (CompilationUnit.GlobalScope.Functions.Length > 0)
        {
            indentedTextWriter.WriteLine("Functions:");
            indentedTextWriter.Indent++;

            foreach ((FunctionSymbol functionSymbol, BoundBlockStatement functionBody) in CompilationUnit.Program.FunctionBodies)
            {
                if (!CompilationUnit.GlobalScope.Functions.Contains(functionSymbol))
                {
                    continue;
                }

                functionSymbol.WriteTo(indentedTextWriter);
                indentedTextWriter.WriteLine();
                functionBody.WriteTo(indentedTextWriter);
                indentedTextWriter.WriteLine();
            }

            indentedTextWriter.Indent--;
        }

        BoundBlockStatement blockStatement = CompilationUnit.GlobalScope.Statement;
        if (blockStatement.Statements.Length > 0)
        {
            indentedTextWriter.WriteLine("Main:");
            indentedTextWriter.Indent++;
            blockStatement.WriteTo(indentedTextWriter);
            indentedTextWriter.Indent--;
        }
    }

    public void EmitGraphVizControlFlow(string controlFlowsDirectory)
    {
        if (Directory.Exists(controlFlowsDirectory))
        {
            Directory.Delete(controlFlowsDirectory, true);
        }

        Directory.CreateDirectory(controlFlowsDirectory);

        foreach ((FunctionSymbol functionSymbol, BoundBlockStatement functionBody) in CompilationUnit.Program.FunctionBodies)
        {
            if (!CompilationUnit.GlobalScope.Functions.Contains(functionSymbol))
            {
                continue;
            }

            ControlFlowGraph functionControlFlowGraph = ControlFlowGraph.Create(functionBody);

            using StreamWriter functionWriter = new(Path.Combine(controlFlowsDirectory, $"{functionSymbol.Name}.dot"));
            functionControlFlowGraph.WriteGraphVizTo(functionWriter);

            functionWriter.Flush();
        }

        ControlFlowGraph globalScopeControlFlowGraph = ControlFlowGraph.Create(CompilationUnit.GlobalScope.Statement);

        using StreamWriter globalScopeWriter = new(Path.Combine(controlFlowsDirectory, "#GlobalScope.dot"));
        globalScopeControlFlowGraph.WriteGraphVizTo(globalScopeWriter);

        globalScopeWriter.Flush();
    }
}
