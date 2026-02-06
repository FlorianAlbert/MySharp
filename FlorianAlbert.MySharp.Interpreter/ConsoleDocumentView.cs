using FlorianAlbert.MySharp.Interpreter.LineRendering;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Interpreter;

internal sealed class ConsoleDocumentView
{
    private const char _minPrintableChar = ' ';
    private const char _maxPrintableChar = '\u007E';
    private const int _tabWidth = 4;

    private readonly Func<string, bool> _submissionChecker;
    private readonly ConsoleKey _forceLineEnterKey;
    private readonly ConsoleModifiers _forceLineEnterModifiers;
    private readonly ImmutableArray<string> _history;

    private readonly ConsoleRenderer _renderer;
    private readonly List<string> _lines = [string.Empty];
    private int _currentHistoryIndex = 0;

    public ConsoleDocumentView(Func<string, bool> submissionChecker, 
        ConsoleKey forceLineEnterKey, 
        ConsoleModifiers forceLineEnterModifiers, 
        ImmutableArray<string> history,
        LineRenderer? lineRenderer = null)
    {
        _submissionChecker = submissionChecker;
        _forceLineEnterKey = forceLineEnterKey;
        _forceLineEnterModifiers = forceLineEnterModifiers;
        _history = history;

        lineRenderer ??= new DefaultLineRenderer();
        _renderer = new(lineRenderer);
        _renderer.Render(Lines, CurrentLineIndex, CurrentCharIndex);
    }

    public string Text => string.Join(Environment.NewLine, _lines);

    public ImmutableArray<string> Lines => [.. _lines];

    public bool IsDone 
    {
        get; 
        private set
        {
            if (value)
            {
                CurrentLineIndex = _lines.Count - 1;
                CurrentCharIndex = _lines[CurrentLineIndex].Length;

                _renderer.UpdateCursorPosition(CurrentLineIndex, CurrentCharIndex);
            }

            field = value;
        }
    }

    public int CurrentLineIndex { get; private set; } = 0;

    public int CurrentCharIndex { get; private set; } = 0;

    // Key Handling

    public void HandleKeyPress(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.Key == _forceLineEnterKey && keyInfo.Modifiers == _forceLineEnterModifiers)
        {
            EnterLine();
            return;
        }

        switch (keyInfo.Key)
        {
            case ConsoleKey.Enter when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleEnter();
                break;
            case ConsoleKey.Tab when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleTab();
                break;
            case ConsoleKey.Backspace when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleBackSpace();
                break;
            case ConsoleKey.Delete when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleDelete();
                break;
            case ConsoleKey.Escape when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleEscape();
                break;
            case ConsoleKey.LeftArrow when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleLeftArrow();
                break;
            case ConsoleKey.RightArrow when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleRightArrow();
                break;
            case ConsoleKey.UpArrow when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleUpArrow();
                break;
            case ConsoleKey.DownArrow when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleDownArrow();
                break;
            case ConsoleKey.Home when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleHome();
                break;
            case ConsoleKey.End when keyInfo.Modifiers is ConsoleModifiers.None:
                HandleEnd();
                break;
            case ConsoleKey.PageUp when keyInfo.Modifiers is ConsoleModifiers.None:
                HandlePageUp();
                break;
            case ConsoleKey.PageDown when keyInfo.Modifiers is ConsoleModifiers.None:
                HandlePageDown();
                break;
            default:
                HandleTyping(keyInfo);
                break;
        }
    }

    private void HandleEnter()
    {
        if (_submissionChecker(Text))
        {
            IsDone = true;
            return;
        }

        EnterLine();
    }

    private void EnterLine()
    {
        string currentLine = _lines[CurrentLineIndex];
        string newLine = currentLine[CurrentCharIndex..];
        string updatedCurrentLine = currentLine[..CurrentCharIndex];

        _lines[CurrentLineIndex] = updatedCurrentLine;
        _lines.Insert(CurrentLineIndex + 1, newLine);

        CurrentLineIndex++;
        CurrentCharIndex = 0;

        _renderer.Render(Lines, CurrentLineIndex, CurrentCharIndex);
    }

    private void HandleTab()
    {
        string currentLine = _lines[CurrentLineIndex];
        int spacesToInsert = _tabWidth - (CurrentCharIndex % _tabWidth);

        _lines[CurrentLineIndex] = currentLine.Insert(CurrentCharIndex, new string(' ', spacesToInsert));
        CurrentCharIndex += spacesToInsert;

        _renderer.Render(Lines, CurrentLineIndex, CurrentCharIndex);
    }

    private void HandleBackSpace()
    {
        if (CurrentCharIndex > 0)
        {
            string currentLine = _lines[CurrentLineIndex];
            _lines[CurrentLineIndex] = currentLine.Remove(CurrentCharIndex - 1, 1);
            CurrentCharIndex--;

            _renderer.Render(Lines, CurrentLineIndex, CurrentCharIndex);
        }
        else if (CurrentLineIndex > 0)
        {
            string currentLine = _lines[CurrentLineIndex];
            string previousLine = _lines[CurrentLineIndex - 1];

            _lines.RemoveAt(CurrentLineIndex);
            CurrentLineIndex--;
            _lines[CurrentLineIndex] = previousLine + currentLine;
            CurrentCharIndex = previousLine.Length;

            _renderer.Render(Lines, CurrentLineIndex, CurrentCharIndex);
        }
        // At document start - nothing to delete
    }

    private void HandleDelete()
    {
        string currentLine = _lines[CurrentLineIndex];
        if (CurrentCharIndex < currentLine.Length)
        {
            _lines[CurrentLineIndex] = currentLine.Remove(CurrentCharIndex, 1);

            _renderer.Render(Lines, CurrentLineIndex, CurrentCharIndex);
        }
        else if (CurrentLineIndex < _lines.Count - 1)
        {
            string nextLine = _lines[CurrentLineIndex + 1];
            _lines[CurrentLineIndex] = currentLine + nextLine;
            _lines.RemoveAt(CurrentLineIndex + 1);

            _renderer.Render(Lines, CurrentLineIndex, CurrentCharIndex);
        }
        // At document end - nothing to delete
    }

    private void HandleEscape()
    {
        _lines[CurrentLineIndex] = string.Empty;
        CurrentCharIndex = 0;

        _renderer.Render(Lines, CurrentLineIndex, CurrentCharIndex);
    }

    private void HandleLeftArrow()
    {
        if (CurrentCharIndex > 0)
        {
            CurrentCharIndex--;

            _renderer.UpdateCursorPosition(CurrentLineIndex, CurrentCharIndex);
        }
        else if (CurrentLineIndex > 0)
        {
            CurrentLineIndex--;
            CurrentCharIndex = _lines[CurrentLineIndex].Length;

            _renderer.UpdateCursorPosition(CurrentLineIndex, CurrentCharIndex);
        }
        // At document start - nothing to move left to
    }

    private void HandleRightArrow()
    {
        if (CurrentCharIndex < _lines[CurrentLineIndex].Length)
        {
            CurrentCharIndex++;

            _renderer.UpdateCursorPosition(CurrentLineIndex, CurrentCharIndex);
        }
        else if (CurrentLineIndex < _lines.Count - 1)
        {
            CurrentLineIndex++;
            CurrentCharIndex = 0;

            _renderer.UpdateCursorPosition(CurrentLineIndex, CurrentCharIndex);
        }
        // At document end - nothing to move right to
    }

    private void HandleUpArrow()
    {
        if (CurrentLineIndex > 0)
        {
            CurrentLineIndex--;
            CurrentCharIndex = Math.Min(CurrentCharIndex, _lines[CurrentLineIndex].Length);

            _renderer.UpdateCursorPosition(CurrentLineIndex, CurrentCharIndex);
        }
        // In first line - nothing to move up to
    }

    private void HandleDownArrow()
    {
        if (CurrentLineIndex < _lines.Count - 1)
        {
            CurrentLineIndex++;
            CurrentCharIndex = Math.Min(CurrentCharIndex, _lines[CurrentLineIndex].Length);

            _renderer.UpdateCursorPosition(CurrentLineIndex, CurrentCharIndex);
        }
        // In last line - nothing to move down to
    }

    private void HandleHome()
    {
        if (CurrentCharIndex > 0)
        {
            CurrentCharIndex = 0;

            _renderer.UpdateCursorPosition(CurrentLineIndex, CurrentCharIndex);
        }
        // At line start - nothing to move home to
    }

    private void HandleEnd()
    {
        int currentLineLength = _lines[CurrentLineIndex].Length;
        if (CurrentCharIndex < currentLineLength)
        {
            CurrentCharIndex = currentLineLength;
            _renderer.UpdateCursorPosition(CurrentLineIndex, CurrentCharIndex);
        }
        // At line end - nothing to move end to
    }

    private void HandlePageUp()
    {
        _currentHistoryIndex--;
        if (_currentHistoryIndex < 0)
        {
            _currentHistoryIndex = _history.Length - 1;
        }

        UpdateDocumentFromHistory();
    }

    private void HandlePageDown()
    {
        _currentHistoryIndex++;
        if (_currentHistoryIndex >= _history.Length)
        {
            _currentHistoryIndex = 0;
        }

        UpdateDocumentFromHistory();
    }

    private void UpdateDocumentFromHistory()
    {
        if (_history.Length == 0)
        {
            return;
        }

        string historyEntry = _history[_currentHistoryIndex];

        _lines.Clear();
        _lines.AddRange(historyEntry.Split(Environment.NewLine));

        CurrentLineIndex = _lines.Count - 1;
        string currentLine = _lines[CurrentLineIndex];

        CurrentCharIndex = currentLine.Length;

        _renderer.Render(Lines, CurrentLineIndex, CurrentCharIndex);
    }

    private void HandleTyping(ConsoleKeyInfo keyInfo)
    {
        if (keyInfo.KeyChar is >= _minPrintableChar and <= _maxPrintableChar)
        {
            string currentLine = _lines[CurrentLineIndex];
            string enteredValue = keyInfo.KeyChar.ToString();
            _lines[CurrentLineIndex] = currentLine.Insert(CurrentCharIndex, enteredValue);
            CurrentCharIndex += enteredValue.Length;

            _renderer.Render(Lines, CurrentLineIndex, CurrentCharIndex);
        }
    }
}
