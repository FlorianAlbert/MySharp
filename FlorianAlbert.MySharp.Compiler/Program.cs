using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Evaluation;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

if (args.Length is 0)
{
    Console.Error.WriteLine("Usage: msc <source-file>");
    return 1;
}

string[] sourceFilePaths = GetFullSourceFilePaths(args);

SyntaxTree[] syntaxTrees = new SyntaxTree[sourceFilePaths.Length];
bool anyFileDoesNotExist = false;
for (int sourceFilePathIndex = 0; sourceFilePathIndex < sourceFilePaths.Length; sourceFilePathIndex++)
{
    string sourceFilePath = sourceFilePaths[sourceFilePathIndex];
    if (!File.Exists(sourceFilePath))
    {
        Console.Error.WriteLine($"Error: The file '{sourceFilePath}' does not exist.");
        anyFileDoesNotExist = true;
        continue;
    }

    // We only load the SyntaxTree for a source file if all previous source files already existed.
    // Otherwise we can skip the overhead of unnecessary loading and parsing of the SyntaxTree,
    // because if any source file does not exist, we won't be able to compile the program anyway
    // and make an early return after the loading loop.
    if (!anyFileDoesNotExist)
    {
        SyntaxTree syntaxTree = SyntaxTree.Load(sourceFilePath);
        syntaxTrees[sourceFilePathIndex] = syntaxTree;
    }
}

if (anyFileDoesNotExist)
{
    return 1;
}

Compilation compilation = new(syntaxTrees);

if (compilation.HasDiagnostics)
{
    compilation.EmitDiagnostics(Console.Error);
    return 1;
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

    return 0;
}

static string[] GetFullSourceFilePaths(IEnumerable<string> paths)
{
    SortedSet<string> sourceFilePaths = [];
    foreach (string path in paths)
    {
        // We trim the trailing double quote from the path, because when we pass a directory
        // path with a trailing backslash to the compiler, the command line parser will interpret
        // the trailing backslash as an escape character for the closing double quote.
        string trimmedPath = path.TrimEnd('"');
        if (Directory.Exists(trimmedPath))
        {
            IEnumerable<string> directorySourceFilePaths = Directory.EnumerateFiles(trimmedPath, "*.ms", SearchOption.AllDirectories);
            sourceFilePaths.UnionWith(directorySourceFilePaths);
            continue;
        }

        sourceFilePaths.Add(Path.GetFullPath(trimmedPath));
    }

    return [.. sourceFilePaths];
}