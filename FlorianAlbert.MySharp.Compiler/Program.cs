using FlorianAlbert.MySharp.Sdk.Evaluator;
using FlorianAlbert.MySharp.Sdk.Parser;
using FlorianAlbert.MySharp.Sdk.Parser.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.Syntax;

bool showParseTree = false;

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

        Binder binder = new();
        BoundExpression boundExpression = binder.BindExpression(syntaxTree.Root);

        IReadOnlyList<Diagnostic> diagnostics = [.. syntaxTree.Diagnostics, .. binder.Diagnostics];

        if (showParseTree)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            PrettyPrint(syntaxTree.Root);
        }

        Console.WriteLine();
        if (diagnostics.Any())
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;

            foreach (Diagnostic diagnostic in diagnostics)
            {
                Console.WriteLine(diagnostic);
            }

            Console.WriteLine();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Magenta;

            Evaluator evaluator = new(boundExpression);
            object? result = evaluator.Evaluate();

            Console.WriteLine(result);
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