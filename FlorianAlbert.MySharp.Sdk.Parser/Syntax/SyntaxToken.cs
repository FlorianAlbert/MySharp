namespace FlorianAlbert.MySharp.Sdk.Parser.Syntax;

public sealed class SyntaxToken : SyntaxNode
{
    public SyntaxToken(SyntaxKind kind, int start, string? text, object? value)
    {
        Kind = kind;
        Span = new(start, text?.Length ?? 0);
        Text = text;
        Value = value;
    }

    public override SyntaxKind Kind { get; }
    
    public string? Text { get; }

    public object? Value { get; }

    public TextSpan Span { get; }

    public override IEnumerable<SyntaxNode> GetChildren() => [];
}