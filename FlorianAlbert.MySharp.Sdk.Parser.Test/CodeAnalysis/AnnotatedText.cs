using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.Test.CodeAnalysis;

internal sealed class AnnotatedText
{
    public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
    {
        Text = text;
        Spans = spans;
    }

    public string Text { get; }

    public ImmutableArray<TextSpan> Spans { get; }

    public static AnnotatedText Parse(string text)
    {
        text = Unindent(text);

        StringBuilder textBuilder = new();
        ImmutableArray<TextSpan>.Builder spanBuilder = ImmutableArray.CreateBuilder<TextSpan>();
        Stack<int> startStack = new();

        int position = 0;
        foreach (char c in text)
        {
            if (c is '[')
            {
                startStack.Push(position);
            }
            else if (c is ']')
            {
                if (startStack.Count is 0)
                {
                    throw new InvalidOperationException("Unmatched closing bracket in annotated text.");
                }
                int start = startStack.Pop();
                int end = position;
                spanBuilder.Add(TextSpan.FromBounds(start, end));
            }
            else
            {
                textBuilder.Append(c);
                position++;
            }
        }

        if (startStack.Count is not 0)
        {
            throw new InvalidOperationException("Unmatched opening bracket in annotated text.");
        }

        return new(textBuilder.ToString(), spanBuilder.ToImmutable());
    }

    private static string Unindent(string text)
    {
        ImmutableArray<string> lines = UnindentLines(text);

        return string.Join(Environment.NewLine, lines);
    }

    public static ImmutableArray<string> UnindentLines(string text)
    {
        ImmutableArray<string>.Builder lines = ImmutableArray.CreateBuilder<string>();
        using StringReader stringReader = new(text);

        string? line;
        while ((line = stringReader.ReadLine()) != null)
        {
            lines.Add(line);
        }

        int minIndentation = int.MaxValue;
        for (int i = 0; i < lines.Count; i++)
        {
            line = lines[i];
            if (line.Trim().Length is 0)
            {
                continue;
            }

            int indentation = line.Length - line.TrimStart().Length;
            minIndentation = Math.Min(minIndentation, indentation);
        }

        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i].Trim().Length is 0)
            {
                continue;
            }

            lines[i] = lines[i][minIndentation..];
        }

        while (lines.Count > 0 && lines[0].Trim().Length is 0)
        {
            lines.RemoveAt(0);
        }

        while (lines.Count > 0 && lines[^1].Trim().Length is 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        return lines.ToImmutable();
    }
}
