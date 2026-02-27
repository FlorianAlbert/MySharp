using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Evaluation;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

if (args.Length is 0)
{
    Console.Error.WriteLine("Usage: msc <source-file>");
    return;
}

if (args.Length > 1)
{
    Console.Error.WriteLine("Error: Too many arguments provided. Please provide only one source file.");
    return;
}

string sourceFilePath = args[0];

string text = File.ReadAllText(sourceFilePath);

SyntaxTree syntaxTree = SyntaxTree.Parse(text);

Compilation compilation = new(syntaxTree);

if (compilation.HasDiagnostics)
{
    compilation.EmitDiagnostics(Console.Out);
}
else
{
    EvaluationResult result = compilation.Evaluate([]);

    if (result.Value is not null)
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(result.Value);
        Console.ResetColor();
    }
}
