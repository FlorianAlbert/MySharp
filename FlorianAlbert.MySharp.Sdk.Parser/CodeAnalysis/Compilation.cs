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
    public Compilation(params IEnumerable<SyntaxTree> syntaxTrees)
        : this(null, syntaxTrees)
    {
    }

    private Compilation(Compilation? previous, params IEnumerable<SyntaxTree> syntaxTrees)
    {
        _Previous = previous;
        SyntaxTrees = [.. syntaxTrees];
    }

    private Compilation? _Previous { get; }

    public ImmutableArray<SyntaxTree> SyntaxTrees { get; }

    public ImmutableArray<FunctionSymbol> Functions => CompilationUnit.GlobalScope.Functions;

    public bool HasDiagnostics => SyntaxTrees.SelectMany(syntaxTree => syntaxTree.Diagnostics).Any() || CompilationUnit.Diagnostics.Length > 0;

    internal BoundCompilationUnit CompilationUnit
    {
        get
        {
            if (field is null)
            {
                BoundCompilationUnit compilationUnit = Binder.BindCompilationUnit(_Previous?.CompilationUnit, SyntaxTrees);
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
        DiagnosticBag diagnostics = [.. SyntaxTrees.SelectMany(syntaxTree => syntaxTree.Diagnostics), .. CompilationUnit.Diagnostics];
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
        DiagnosticBag diagnostics = [.. SyntaxTrees.SelectMany(syntaxTree => syntaxTree.Diagnostics), .. CompilationUnit.Diagnostics];
        Diagnostic[] orderedDiagnostics = [.. diagnostics.OrderBy(diagnostic => diagnostic.Location.FileName)
                                                         .ThenBy(diagnostic => diagnostic.Location.Span.Start)
                                                         .ThenBy(diagnostic => diagnostic.Location.Span.Length)];

        for (int diagnosticIndex = 0; diagnosticIndex < orderedDiagnostics.Length; diagnosticIndex++)
        {
            Diagnostic diagnostic = orderedDiagnostics[diagnosticIndex];

            if (diagnosticIndex > 0)
            {
                textWriter.WriteLine();
            }

            string fileName = diagnostic.Location.FileName;
            int lineIndexStart = diagnostic.Location.StartLineIndex;
            int lineIndexEnd = diagnostic.Location.EndLineIndex;
            TextLine lineStart = diagnostic.Location.SourceText.Lines[lineIndexStart];
            TextLine lineEnd = diagnostic.Location.SourceText.Lines[lineIndexEnd];

            int lineNumber = lineIndexStart + 1;
            int characterNumber = diagnostic.Location.StartCharacterIndex + 1;

            textWriter.SetForegroundColor(ConsoleColor.DarkRed);
            textWriter.Write($"{fileName}({lineNumber}, {characterNumber}): ");
            textWriter.WriteLine(diagnostic);
            textWriter.ResetColor();

            textWriter.WriteLine();

            TextSpan prefixSpan = TextSpan.FromBounds(lineStart.Start, diagnostic.Location.Span.Start);
            TextSpan suffixSpan = TextSpan.FromBounds(diagnostic.Location.Span.End, lineEnd.End);

            string prefix = diagnostic.Location.SourceText.ToString(prefixSpan);
            string error = diagnostic.Location.SourceText.ToString(diagnostic.Location.Span);
            string suffix = diagnostic.Location.SourceText.ToString(suffixSpan);

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

    public void EmitTree(FunctionSymbol functionSymbol, TextWriter writer)
    {
        IndentedTextWriter indentedTextWriter = writer as IndentedTextWriter ?? new IndentedTextWriter(writer);
        BoundCompilationUnit? compilationUnit = CompilationUnit;
        BoundBlockStatement? functionBody = null;
        while (compilationUnit is not null && !compilationUnit.Program.FunctionBodies.TryGetValue(functionSymbol, out functionBody))
        {
            compilationUnit = compilationUnit.Previous;
        }

        if (functionBody is null)
        {
            throw new InvalidOperationException($"Function '{functionSymbol.Name}' not found in any compilation unit.");
        }

        functionSymbol.WriteTo(indentedTextWriter);
        indentedTextWriter.WriteLine();
        functionBody.WriteTo(indentedTextWriter);
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
