namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

public sealed class TextLocation
{
    public TextLocation(SourceText sourceText, TextSpan span)
    {
        SourceText = sourceText;
        Span = span;
    }

    public string FileName => SourceText.FileName;

    public SourceText SourceText { get; }

    public TextSpan Span { get; }

    public int StartLineIndex => SourceText.GetLineIndex(Span.Start);

    public int EndLineIndex => SourceText.GetLineIndex(Span.End);

    public int StartCharacterIndex => Span.Start - SourceText.Lines[StartLineIndex].Start;

    public int EndCharacterIndex => Span.End - SourceText.Lines[EndLineIndex].Start;
}
