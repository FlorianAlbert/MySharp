using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Evaluation;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
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

    public bool HasDiagnostics => SyntaxTree.Diagnostics.Length > 0 || CompilationUnit.Diagnostics.Length > 0;

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

    public void EmitDiagnostics(TextWriter textWriter)
    {
        DiagnosticBag diagnostics = [.. SyntaxTree.Diagnostics, .. CompilationUnit.Diagnostics];
        Diagnostic[] orderedDiagnostics = [.. diagnostics.OrderBy(diagnostic => diagnostic.Span, TextSpan.Comparer)];

        for (int diagnosticIndex = 0; diagnosticIndex < orderedDiagnostics.Length; diagnosticIndex++)
        {
            Diagnostic diagnostic = orderedDiagnostics[diagnosticIndex];

            if (diagnosticIndex > 0)
            {
                textWriter.WriteLine();
            }

            int lineIndexStart = SyntaxTree.SourceText.GetLineIndex(diagnostic.Span.Start);
            int lineIndexEnd = SyntaxTree.SourceText.GetLineIndex(diagnostic.Span.End);
            int lineNumber = lineIndexStart + 1;
            TextLine lineStart = SyntaxTree.SourceText.Lines[lineIndexStart];
            TextLine lineEnd = SyntaxTree.SourceText.Lines[lineIndexEnd];
            int characterLineIndex = diagnostic.Span.Start - lineStart.Start + 1;

            textWriter.SetForegroundColor(ConsoleColor.DarkRed);
            textWriter.Write($"({lineNumber}, {characterLineIndex}): ");
            textWriter.WriteLine(diagnostic);
            textWriter.ResetColor();

            textWriter.WriteLine();

            TextSpan prefixSpan = TextSpan.FromBounds(lineStart.Start, diagnostic.Span.Start);
            TextSpan suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, lineEnd.End);

            string prefix = SyntaxTree.SourceText.ToString(prefixSpan);
            string error = SyntaxTree.SourceText.ToString(diagnostic.Span);
            string suffix = SyntaxTree.SourceText.ToString(suffixSpan);

            textWriter.Write(prefix);
            textWriter.SetForegroundColor(ConsoleColor.DarkRed);
            textWriter.Write(error);
            textWriter.ResetColor();
            textWriter.WriteLine(suffix);
        }
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
