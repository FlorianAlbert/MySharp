namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

public sealed class TextLine
{
    public TextLine(SourceText sourceText, int start, int length, int lengthIncludingLineBreak)
    {
        SourceText = sourceText;
        Start = start;
        Length = length;
        LengthIncludingLineBreak = lengthIncludingLineBreak;
    }

    public SourceText SourceText { get; }

    public int Start { get; }

    public int Length { get; }

    public int End => Start + Length;

    public int LengthIncludingLineBreak { get; }

    public TextSpan Span => new(Start, Length);

    public TextSpan SpanIncludingLineBreak => new(Start, LengthIncludingLineBreak);

    public override string ToString()
    {
        return SourceText.ToString(Span);
    }
}
