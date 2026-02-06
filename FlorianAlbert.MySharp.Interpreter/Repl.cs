using FlorianAlbert.MySharp.Interpreter.LineRendering;

namespace FlorianAlbert.MySharp.Interpreter;

internal abstract class Repl
{
    protected const char _metaCommandPrefix = '/';
    private const string _exitString = "";

    private readonly List<string> _history = [];
    private readonly LineRenderer? _lineRenderer;

    protected Repl(LineRenderer? lineRenderer = null)
    {
        _lineRenderer = lineRenderer;
    }

    public void Run()
    {
        while (true)
        {
            string submission = GetSubmission();

            if (submission is _exitString)
            {
                return;
            }

            EvaluateSubmission(submission);

            _history.Add(submission);
        }
    }

    private string GetSubmission()
    {
        ConsoleDocumentView documentView = new(IsCompleteSubmission, ConsoleKey.Enter, ConsoleModifiers.Control, [.. _history], _lineRenderer);
        while (!documentView.IsDone)
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            documentView.HandleKeyPress(keyInfo);
        }

        Console.WriteLine();

        return documentView.Text;
    }

    protected virtual void EvaluateMetaCommand(string input)
    {
        if (input.Equals($"{_metaCommandPrefix}cls", StringComparison.OrdinalIgnoreCase))
        {
            Console.Clear();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"Unknown command '{input}'.");
            Console.ResetColor();
        }
    }

    protected virtual bool IsCompleteSubmission(string text)
    {
        if (text.StartsWith(_metaCommandPrefix))
        {
            return true;
        }

        if (text is _exitString)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Evaluates the given submission. Returns true if the submission was evaluated, otherwise false.
    /// If overridden, the overriding method should call base.EvaluateSubmission to ensure meta-commands are handled.
    /// Only if submission is not already handled by the base implementation, the overriding method should
    /// evaluate the submission and return true if it was evaluated.
    /// </summary>
    /// <param name="text">The text to evaluate.</param>
    /// <returns>Boolean value indicating whether the submission was evaluated.</returns>
    protected virtual bool EvaluateSubmission(string text)
    {
        if (text.StartsWith(_metaCommandPrefix))
        {
            EvaluateMetaCommand(text);
            return true;
        }

        return false;
    }
}
