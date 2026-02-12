using FlorianAlbert.MySharp.Interpreter.LineRendering;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Interpreter;

internal sealed class MySharpRepl : Repl
{
    private Compilation? _previousCompilation;
    private bool _showSyntaxTree;
    private bool _showBoundTree;
    private readonly Dictionary<VariableSymbol, object?> _variables = [];

    public MySharpRepl() : base(new MySharpLineRenderer())
    {
    }

    protected override void EvaluateMetaCommand(string input)
    {
        if (input.Equals($"{_metaCommandPrefix}showSyntaxTree", StringComparison.OrdinalIgnoreCase) || input.Equals($"{_metaCommandPrefix}sst", StringComparison.OrdinalIgnoreCase))
        {
            _showSyntaxTree = !_showSyntaxTree;
            Console.WriteLine(_showSyntaxTree ? "Showing syntax tree" : "Not showing syntax tree");
        }
        else if (input.Equals($"{_metaCommandPrefix}showBoundTree", StringComparison.OrdinalIgnoreCase) || input.Equals($"{_metaCommandPrefix}sbt", StringComparison.OrdinalIgnoreCase))
        {
            _showBoundTree = !_showBoundTree;
            Console.WriteLine(_showBoundTree ? "Showing bound tree" : "Not showing bound tree");
        }
        else if (input.Equals($"{_metaCommandPrefix}reset", StringComparison.OrdinalIgnoreCase))
        {
            _previousCompilation = null;
            _variables.Clear();
            Console.WriteLine("Resetting compilation.");
        }
        else
        {
            base.EvaluateMetaCommand(input);
        }
    }

    protected override bool IsCompleteSubmission(string text)
    {
        if (base.IsCompleteSubmission(text))
        {
            return true;
        }

        bool lastTwoLinesAreEmpty = text.Split(Environment.NewLine).Reverse().Take(2).All(string.IsNullOrWhiteSpace);
        if (lastTwoLinesAreEmpty)
        {
            return true;
        }

        SyntaxTree syntaxTree = SyntaxTree.Parse(text);

        return !syntaxTree.Diagnostics.Any();
    }

    /// <summary>
    /// Evaluates the given submission. Returns true if the submission was evaluated, otherwise false.
    /// </summary>
    /// <param name="text">The text to evaluate.</param>
    /// <returns>Boolean value indicating whether the submission was evaluated.</returns>
    protected override bool EvaluateSubmission(string text)
    {
        if (base.EvaluateSubmission(text))
        {
            return true;
        }

        var syntaxTree = SyntaxTree.Parse(text);

        Compilation compilation = _previousCompilation is null ?
            new(syntaxTree) :
            _previousCompilation.ContinueWith(syntaxTree);

        if (_showSyntaxTree)
        {
            Console.WriteLine();
            Console.WriteLine("Syntax tree:");
            syntaxTree.Root.WriteTo(Console.Out);
        }

        if (_showBoundTree)
        {
            Console.WriteLine();
            Console.WriteLine("Bound tree:");
            compilation.EmitTree(Console.Out);
        }

        EvaluationResult result = compilation.Evaluate(_variables);

        if (result.Diagnostics.Length > 0)
        {
            foreach (Diagnostic diagnostic in result.Diagnostics)
            {
                Console.WriteLine();

                int lineIndex = syntaxTree.SourceText.GetLineIndex(diagnostic.Span.Start);
                int lineNumber = lineIndex + 1;
                TextLine line = syntaxTree.SourceText.Lines[lineIndex];
                int characterLineIndex = diagnostic.Span.Start - line.Start + 1;

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write($"({lineNumber}, {characterLineIndex}): ");
                Console.WriteLine(diagnostic);
                Console.ResetColor();

                TextSpan prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                TextSpan suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                string prefix = syntaxTree.SourceText.ToString(prefixSpan);
                string error = syntaxTree.SourceText.ToString(diagnostic.Span);
                string suffix = syntaxTree.SourceText.ToString(suffixSpan);

                Console.Write($"\t{prefix}");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write(error);
                Console.ResetColor();
                Console.WriteLine(suffix);
            }

            Console.WriteLine();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(result.Value);
            Console.ResetColor();

            _previousCompilation = compilation;
        }

        return true;
    }
}
