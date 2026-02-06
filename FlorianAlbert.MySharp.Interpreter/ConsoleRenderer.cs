using FlorianAlbert.MySharp.Interpreter.LineRendering;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Interpreter;

internal sealed class ConsoleRenderer
{
    private const int _linePrefixLength = 2;
    private const string _primaryPrompt = "\u00BB";   // »
    private const string _continuationPrompt = "\u00B7"; // ·

    private readonly LineRenderer _lineRenderer;
    private int _renderedCursorLineIndex;
    private int _renderedLineCount;

    public ConsoleRenderer(LineRenderer lineRenderer)
    {
        _lineRenderer = lineRenderer;
    }

    public void Render(ImmutableArray<string> lines, int cursorLineIndex, int cursorCharIndex)
    {
        Console.CursorVisible = false;

        int firstLineIndex = Console.CursorTop - _renderedCursorLineIndex;
        Console.SetCursorPosition(0, firstLineIndex);

        RenderLines(lines);
        ClearRemainingLines(lines.Length);

        Console.SetCursorPosition(cursorCharIndex + _linePrefixLength, firstLineIndex + cursorLineIndex);
        Console.CursorVisible = true;

        _renderedCursorLineIndex = cursorLineIndex;
        _renderedLineCount = lines.Length;
    }

    public void UpdateCursorPosition(int lineIndex, int charIndex)
    {
        int firstLineIndex = Console.CursorTop - _renderedCursorLineIndex;
        Console.SetCursorPosition(charIndex + _linePrefixLength, firstLineIndex + lineIndex);

        _renderedCursorLineIndex = lineIndex;
    }

    public void Reset()
    {
        _renderedCursorLineIndex = 0;
        _renderedLineCount = 0;
    }

    private void RenderLines(ImmutableArray<string> lines)
    {
        int availableWidth = GetAvailableLineWidth();

        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            RenderLinePrefix(lineIndex);
            RenderLineContent(lines[lineIndex], availableWidth);
        }
    }

    private void RenderLinePrefix(int lineIndex)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        string prompt = lineIndex == 0 ? _primaryPrompt : _continuationPrompt;
        Console.Write(prompt.PadRight(_linePrefixLength));
        Console.ResetColor();
    }

    private void RenderLineContent(string line, int availableWidth)
    {
        _lineRenderer.RenderLine(line);

        int remainingSpaces;
        if (availableWidth > 0 && availableWidth > line.Length)
        {
            remainingSpaces = availableWidth - line.Length;
        }
        else
        {
            remainingSpaces = 0;
        }

        Console.WriteLine(new string(' ', remainingSpaces));
    }

    private void ClearRemainingLines(int currentLineCount)
    {
        if (_renderedLineCount <= currentLineCount)
        {
            return;
        }

        int linesToClear = _renderedLineCount - currentLineCount;
        string emptyLine = new(' ', Console.WindowWidth);

        for (int i = 0; i < linesToClear; i++)
        {
            Console.WriteLine(emptyLine);
        }
    }

    private static int GetAvailableLineWidth()
    {
        int windowWidth = Console.WindowWidth;
        return windowWidth > _linePrefixLength ? windowWidth - _linePrefixLength : 0;
    }
}
