using FlorianAlbert.MySharp.Interpreter.Annotations;
using FlorianAlbert.MySharp.Interpreter.LineRendering;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Evaluation;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

namespace FlorianAlbert.MySharp.Interpreter;

[MetaCommandEvaluator(nameof(EvaluateMetaCommand))]
internal sealed partial class MySharpRepl : Repl
{
    private Compilation? _previousCompilation;
    private bool _showSyntaxTree;
    private bool _showBoundTree;
    private bool _emitControlFlows;
    private readonly Dictionary<VariableSymbol, object?> _variables = [];

    public MySharpRepl() : base(new MySharpLineRenderer())
    {
    }

    //protected override void EvaluateMetaCommand(string input)
    //{
    //    if (input.Equals($"{_metaCommandPrefix}showSyntaxTree", StringComparison.OrdinalIgnoreCase) || input.Equals($"{_metaCommandPrefix}sst", StringComparison.OrdinalIgnoreCase))
    //    {
    //        _showSyntaxTree = !_showSyntaxTree;
    //        Console.WriteLine(_showSyntaxTree ? "Showing syntax tree" : "Not showing syntax tree");
    //    }
    //    else if (input.Equals($"{_metaCommandPrefix}showBoundTree", StringComparison.OrdinalIgnoreCase) || input.Equals($"{_metaCommandPrefix}sbt", StringComparison.OrdinalIgnoreCase))
    //    {
    //        _showBoundTree = !_showBoundTree;
    //        Console.WriteLine(_showBoundTree ? "Showing bound tree" : "Not showing bound tree");
    //    }
    //    else if (input.Equals($"{_metaCommandPrefix}emitControlFlows", StringComparison.OrdinalIgnoreCase) || input.Equals($"{_metaCommandPrefix}ecf", StringComparison.OrdinalIgnoreCase))
    //    {
    //        _emitControlFlows = !_emitControlFlows;
    //        Console.WriteLine(_emitControlFlows ? "Storing control flow graphs" : "Not storing control flow graphs");
    //    }
    //    else if (input.Equals($"{_metaCommandPrefix}reset", StringComparison.OrdinalIgnoreCase))
    //    {
    //        _previousCompilation = null;
    //        _variables.Clear();
    //        Console.WriteLine("Resetting compilation.");
    //    }
    //    else
    //    {
    //        base.EvaluateMetaCommand(input);
    //    }
    //}

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
        }

        return true;
    }
}
