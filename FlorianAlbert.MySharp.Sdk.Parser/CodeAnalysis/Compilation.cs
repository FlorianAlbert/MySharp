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
    private Compilation(bool isScript, Compilation? previous, IEnumerable<SyntaxTree> syntaxTrees)
    {
        IsScript = isScript;
        _Previous = previous;
        SyntaxTrees = [.. syntaxTrees];
    }

    public static Compilation Create(params IEnumerable<SyntaxTree> syntaxTrees)
    {
        return new(false, null, syntaxTrees);
    }

    public static Compilation CreateScript(Compilation? previous, params IEnumerable<SyntaxTree> syntaxTrees)
    {
        return new(true, previous, syntaxTrees);
    }

    public bool IsScript { get; }

    private Compilation? _Previous { get; }

    public ImmutableHashSet<SyntaxTree> SyntaxTrees { get; }

    public ImmutableHashSet<Symbol> Symbols => GetOrCreateCombinedSymbols(ref field, CompilationUnit.GlobalScope.Symbols, _Previous?.Symbols ?? []);

    public ImmutableHashSet<FunctionSymbol> Functions => GetOrCreateCombinedSymbols(ref field, CompilationUnit.GlobalScope.Functions, _Previous?.Functions ?? []);

    public ImmutableHashSet<VariableSymbol> Variables => GetOrCreateCombinedSymbols(ref field, CompilationUnit.GlobalScope.Variables, _Previous?.Variables ?? []);

    private ImmutableHashSet<TSymbol> GetOrCreateCombinedSymbols<TSymbol>(ref ImmutableHashSet<TSymbol>? field, ImmutableHashSet<TSymbol> currentCompilationUnitSymbols, ImmutableHashSet<TSymbol> previousCompilationUnitSymbols)
        where TSymbol : Symbol
    {
        field ??= GetAllSymbols(currentCompilationUnitSymbols, previousCompilationUnitSymbols);

        return field;
    }

    private ImmutableHashSet<TSymbol> GetAllSymbols<TSymbol>(ImmutableHashSet<TSymbol> currentCompilationUnitSymbols, ImmutableHashSet<TSymbol> previousCompilationUnitSymbols)
        where TSymbol : Symbol
    {
        ImmutableHashSet<TSymbol>.Builder builder = ImmutableHashSet.CreateBuilder<TSymbol>();
        HashSet<string> seenSymbolNames = [.. currentCompilationUnitSymbols.Select(symbol => symbol.Name)];

        builder.UnionWith(currentCompilationUnitSymbols);

        if (_Previous is null)
        {
            builder.UnionWith(Symbol.BuiltIns.GetAll().OfType<TSymbol>().Where(symbol => seenSymbolNames.Add(symbol.Name)));
        }
        else
        {
            builder.UnionWith(previousCompilationUnitSymbols.Where(symbol => seenSymbolNames.Add(symbol.Name)));
        }

        return builder.ToImmutable();
    }

    public bool HasDiagnostics => SyntaxTrees.SelectMany(syntaxTree => syntaxTree.Diagnostics).Any() || CompilationUnit.Diagnostics.Length > 0;

    internal BoundCompilationUnit CompilationUnit
    {
        get
        {
            if (field is null)
            {
                BoundCompilationUnit compilationUnit = Binder.BindCompilationUnit(IsScript, _Previous?.CompilationUnit, SyntaxTrees);
                Interlocked.CompareExchange(ref field, compilationUnit, null);
            }

            return field;
        }
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

        if (CompilationUnit.GlobalScope.Functions.Count > 0)
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

        if (FunctionSymbol.BuiltIns.GetAll().Contains(functionSymbol))
        {
            functionSymbol.WriteTo(indentedTextWriter);
            indentedTextWriter.WriteLine();

            return;
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
