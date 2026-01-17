using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

bool showParseTree = false;
Dictionary<VariableSymbol, object?> variables = [];

while (true)
{
    Console.Write("> ");

    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
    {
        return;
    }

    if (input.Equals("#showSyntaxTree", StringComparison.OrdinalIgnoreCase) || input.Equals("#sst", StringComparison.OrdinalIgnoreCase))
    {
        showParseTree = !showParseTree;
        Console.WriteLine(showParseTree ? "Showing parse trees" : "Not showing parse trees");
        continue;
    }
    else if (input == "#cls")
    {
        Console.Clear();
        continue;
    }

    Console.WriteLine();

    if (input is not null)
    {
        var syntaxTree = SyntaxTree.Parse(input);

        if (showParseTree)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            syntaxTree.Root.WriteTo(Console.Out);
        }

        Compilation compilation = new(syntaxTree);
        EvaluationResult result = compilation.Evaluate(variables);

        Console.WriteLine();
        if (result.Diagnostics.Length > 0)
        {
            foreach (Diagnostic diagnostic in result.Diagnostics)
            {
                int lineIndex = syntaxTree.SourceText.GetLineIndex(diagnostic.Span.Start);
                int lineNumber = lineIndex + 1;
                TextLine line = syntaxTree.SourceText.Lines[lineIndex];
                int characterLineIndex = diagnostic.Span.Start - line.Start + 1;

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write($"({lineNumber}, {characterLineIndex}): ");
                Console.WriteLine(diagnostic);
                Console.ResetColor();

                string prefix = input[..diagnostic.Span.Start];
                string error = input[diagnostic.Span.Start..diagnostic.Span.End];
                string suffix = input[diagnostic.Span.End..];

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
            Console.ForegroundColor = ConsoleColor.Magenta;

            Console.WriteLine(result.Value);
        }

        Console.ResetColor();

        Console.WriteLine();
    }
}