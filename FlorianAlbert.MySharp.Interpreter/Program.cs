using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

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
            PrettyPrint(syntaxTree.Root);
        }

        Compilation compilation = new(syntaxTree);
        EvaluationResult result = compilation.Evaluate(variables);

        Console.WriteLine();
        if (result.Diagnostics.Count > 0)
        {
            foreach (Diagnostic diagnostic in result.Diagnostics)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
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

static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
{
    string indentToWrite = indent;
    if (isLast)
    {
        indentToWrite += "└── ";
        indent += "    ";
    }
    else
    {
        indentToWrite += "├── ";
        indent += "│   ";
    }

    Console.Write(indentToWrite);
    Console.Write(node.Kind);

    if (node is SyntaxToken token && token.Value is not null)
    {
        Console.Write("    ");
        Console.Write(token.Value);
    }

    Console.WriteLine();

    SyntaxNode? lastChild = node.GetChildren().LastOrDefault();
    foreach (SyntaxNode child in node.GetChildren())
    {
        PrettyPrint(child, indent, child == lastChild);
    }
}