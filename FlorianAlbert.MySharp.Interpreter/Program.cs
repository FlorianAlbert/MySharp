using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Text;

bool showParseTree = false;
Dictionary<VariableSymbol, object?> variables = [];
StringBuilder textBuilder = new();

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    if (textBuilder.Length == 0)
    {
        Console.Write("\u00BB ");
    }
    else
    {
        Console.Write("\u00B7 ");
    }
    Console.ResetColor();

    string? input = Console.ReadLine();

    if (textBuilder.Length == 0)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            break;
        }
        else if (input.Equals("#showSyntaxTree", StringComparison.OrdinalIgnoreCase) || input.Equals("#sst", StringComparison.OrdinalIgnoreCase))
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
    }

    textBuilder.AppendLine(input);
    string currentText = textBuilder.ToString();

    var syntaxTree = SyntaxTree.Parse(currentText);

    if (!string.IsNullOrWhiteSpace(input) && syntaxTree.Diagnostics.Length > 0)
    {
        continue;
    }

    Compilation compilation = new(syntaxTree);
    EvaluationResult result = compilation.Evaluate(variables);

    if (showParseTree)
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        syntaxTree.Root.WriteTo(Console.Out);
        Console.ResetColor();
    }

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
        Console.ForegroundColor = ConsoleColor.Magenta;

        Console.WriteLine(result.Value);
    }

    textBuilder.Clear();

    Console.ResetColor();
}