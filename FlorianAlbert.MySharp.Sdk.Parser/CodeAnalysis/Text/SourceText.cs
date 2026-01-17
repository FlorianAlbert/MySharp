using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

public sealed class SourceText
{
    private readonly string _text;

    private SourceText(string text)
    {
        _text = text;
        Lines = ParseLines(this, text);
    }

    public ImmutableArray<TextLine> Lines { get; }

    public char this[int index] => _text[index];

    public int Length => _text.Length;

    public static SourceText From(string text) => new(text);

    public int GetLineIndex(int position)
    {
        int lowerLineIndex = 0;
        int upperLineIndex = Lines.Length - 1;

        while (lowerLineIndex <= upperLineIndex)
        {
            int currentLineIndex = lowerLineIndex + (upperLineIndex - lowerLineIndex) / 2;
            TextLine currentLine = Lines[currentLineIndex];
            if (position < currentLine.Start)
            {
                upperLineIndex = currentLineIndex - 1;
            }
            else if (position >= currentLine.End)
            {
                lowerLineIndex = currentLineIndex + 1;
            }
            else
            {
                return currentLineIndex;
            }
        }

        return lowerLineIndex - 1;
    }

    private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
    {
        ImmutableArray<TextLine>.Builder resultBuilder = ImmutableArray.CreateBuilder<TextLine>();

        int position = 0;
        int lineStart = 0;

        while (position < text.Length)
        {
            int lineBreakLength = GetLineBreakLength(text, position);

            if (lineBreakLength == 0)
            {
                position++;
            }
            else
            {
                AddLine(resultBuilder, sourceText, position, lineStart, lineBreakLength);

                position += lineBreakLength;
                lineStart = position;
            }
        }

        if (position >= lineStart)
        {
            AddLine(resultBuilder, sourceText, position, lineStart, 0);
        }

        return resultBuilder.ToImmutable();
    }

    private static int GetLineBreakLength(string text, int charIndex)
    {
        char currentCharacter = text[charIndex];
        char lookAheadCharacter = charIndex + 1 >= text.Length ? '\0' : text[charIndex + 1];

        if (currentCharacter is '\r' && lookAheadCharacter is '\n')
        {
            return 2;
        }

        if (currentCharacter is '\r' or '\n')
        {
            return 1;
        }

        return 0;
    }

    private static void AddLine(ImmutableArray<TextLine>.Builder builder, SourceText sourceText, int position, int lineStart, int lineBreakLength)
    {
        int lineLength = position - lineStart;
        int lineLengthIncludingLineBreak = lineLength + lineBreakLength;
        TextLine line = new(sourceText, lineStart, lineLength, lineLengthIncludingLineBreak);

        builder.Add(line);
    }

    public override string ToString()
    {
        return _text;
    }

    public string ToString(int start, int length)
    {
        return _text.Substring(start, length);
    }

    public string ToString(TextSpan span)
    {
        return ToString(span.Start, span.Length);
    }
}
