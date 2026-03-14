using FlorianAlbert.MySharp.Interpreter.Annotations;
using FlorianAlbert.MySharp.Interpreter.LineRendering;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Evaluation;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.Extensions;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Interpreter;

[MetaCommandEvaluator(nameof(EvaluateMetaCommand), nameof(GetAvailableMetaCommandInfos))]
internal sealed partial class MySharpRepl : Repl
{
    private bool _saveSubmissions = true;
    private Compilation? _previousCompilation;
    private bool _showSyntaxTree;
    private bool _showBoundTree;
    private bool _emitControlFlows;
    private readonly Dictionary<VariableSymbol, object?> _variables = [];

    public MySharpRepl() : base(new MySharpLineRenderer())
    {
        LoadSubmissions();
    }

    public override partial void EvaluateMetaCommand(string input);

    [MetaCommand("showSyntaxTree", "Toggles displaying of the syntax tree of the submission.", Aliases = ["sst"])]
    private void EvaluateMetaCommand_ShowSyntaxTree()
    {
        _showSyntaxTree = !_showSyntaxTree;
        Console.WriteLine(_showSyntaxTree ? "Showing syntax tree" : "Not showing syntax tree");
    }

    [MetaCommand("showBoundTree", "Toggles displaying of the bound tree of the submission.", Aliases = ["sbt"])]
    private void EvaluateMetaCommand_ShowBoundTree()
    {
        _showBoundTree = !_showBoundTree;
        Console.WriteLine(_showBoundTree ? "Showing bound tree" : "Not showing bound tree");
    }

    [MetaCommand("emitControlFlows", "Toggles emission of the control flows of the submission as GraphViz diagrams.", Aliases = ["ecf"])]
    private void EvaluateMetaCommand_EmitControlFlows()
    {
        _emitControlFlows = !_emitControlFlows;
        Console.WriteLine(_emitControlFlows ? "Storing control flow graphs" : "Not storing control flow graphs");
    }

    [MetaCommand("reset", "Resets the current session.")]
    private void EvaluateMetaCommand_Reset()
    {
        _previousCompilation = null;
        _variables.Clear();

        ClearSubmissions();

        Console.WriteLine("Resetting compilation.");
    }

    [MetaCommand("dump", "Displays the bound tree of the given function.")]
    private void EvaluateMetaCommand_Dump(string symbolName)
    {
        FunctionSymbol? function = _previousCompilation?.Functions.SingleOrDefault(symbol => symbol.Name.Equals(symbolName, StringComparison.Ordinal));
        if (function is null)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine($"Unknown function: {symbolName}");
            Console.ResetColor();
            return;
        }

        _previousCompilation!.EmitTree(function, Console.Out);
    }

    [MetaCommand("load", "Loads and evaluates the MySharp code in the given file.")]
    private void EvaluateMetaCommand_Load(string filePath)
    {
        filePath = Path.GetFullPath(filePath);
        if (!File.Exists(filePath))
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine($"File not found: '{filePath}'");
            Console.ResetColor();
            return;
        }

        string code = File.ReadAllText(filePath);
        EvaluateSubmission(code);
    }

    [MetaCommand("ls", "Lists all loaded symbols.")]
    private void EvaluateMetaCommand_ListSymbols()
    {
        ImmutableArray<Symbol> symbols = _previousCompilation?.Symbols ?? Symbol.BuiltIns.GetAll();
        foreach (Symbol symbol in symbols.OrderBy(symbol => symbol.Kind).ThenBy(symbol => symbol.Name))
        {
            symbol.WriteTo(Console.Out);
            Console.WriteLine();
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

        SyntaxTree syntaxTree = SyntaxTree.Parse(text);

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

        if (_emitControlFlows)
        {
            string appPath = Environment.GetCommandLineArgs()[0];
            string appDirectory = Path.GetDirectoryName(appPath)!;
            string controlFlowsDirectory = Path.Combine(appDirectory, "ControlFlows");

            Console.WriteLine();
            Console.WriteLine($"Control flow graphs stored at: {controlFlowsDirectory}");
            compilation.EmitGraphVizControlFlow(controlFlowsDirectory);
        }

        if (compilation.HasDiagnostics)
        {
            compilation.EmitDiagnostics(Console.Error);

            Console.Error.WriteLine();
        }
        else
        {
            EvaluationResult result = compilation.Evaluate(_variables);

            if (result.Value is not null)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(result.Value);
                Console.ResetColor();
            }

            _previousCompilation = compilation;

            SaveSubmission(text);
        }

        return true;
    }

    private void SaveSubmission(string text)
    {
        if (!_saveSubmissions)
        {
            return;
        }

        string submissionsDirectory = GetSubmissionsPath();
        Directory.CreateDirectory(submissionsDirectory);

        int submissionIndex = Directory.GetFiles(submissionsDirectory).Length;
        string submissionFilePath = Path.Combine(submissionsDirectory, $"submission{submissionIndex:0000}.ms");

        File.WriteAllText(submissionFilePath, text);
    }

    private static void ClearSubmissions()
    {
        string submissionsDirectory = GetSubmissionsPath();
        if (Directory.Exists(submissionsDirectory))
        {
            Directory.Delete(submissionsDirectory, recursive: true);
        }
    }

    private void LoadSubmissions()
    {
        string submissionsDirectory = GetSubmissionsPath();
        if (!Directory.Exists(submissionsDirectory))
        {
            return;
        }

        _saveSubmissions = false;

        try
        {
            List<string> submissionFiles = [.. Directory.GetFiles(submissionsDirectory).OrderBy(file => file)];
            foreach (string submissionFile in submissionFiles)
            {
                string submissionText = File.ReadAllText(submissionFile);
                EvaluateSubmission(submissionText);
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Loaded {submissionFiles.Count} submissions from previous sessions.");
            Console.ResetColor();
        }
        finally
        {
            _saveSubmissions = true;
        }
    }

    private static string GetSubmissionsPath()
    {
        string localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string submissionsDirectory = Path.Combine(localAppDataPath, "MySharp", "Submissions");
        return submissionsDirectory;
    }
}
