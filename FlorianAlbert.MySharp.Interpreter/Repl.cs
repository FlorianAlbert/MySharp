using System.Collections.ObjectModel;
using System.Text;

namespace FlorianAlbert.MySharp.Interpreter;

internal abstract class Repl
{
    public void Run()
    {
        while (true)
        {
            string? submission = GetSubmission();

            if (submission is null)
            {
                break;
            }

            EvaluateSubmission(submission);
        }
    }

    private string? GetSubmission()
    {
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
                    return null;
                }
                
                if (input.StartsWith('#'))
                {
                    EvaluateMetaCommand(input);
                    continue;
                }
            }

            textBuilder.AppendLine(input);
            string currentText = textBuilder.ToString();

            if (!IsCompleteSubmission(currentText) && !string.IsNullOrWhiteSpace(input))
            {
                continue;
            }

            return currentText;
        }
    }

    protected virtual void EvaluateMetaCommand(string input)
    {
        if (input.Equals("#cls", StringComparison.OrdinalIgnoreCase))
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

    protected abstract bool IsCompleteSubmission(string text);

    protected abstract void EvaluateSubmission(string currentText);
}
