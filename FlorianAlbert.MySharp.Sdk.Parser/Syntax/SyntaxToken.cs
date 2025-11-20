namespace FlorianAlbert.MySharp.Syntax;

public sealed class SyntaxToken : SyntaxNode
{
    public SyntaxToken(SyntaxKind kind, int start, string? text, object? value)
    {
        Kind = kind;
        Start = start;
        Text = text;
        Value = value;
    }

    public override SyntaxKind Kind { get; }

    public int Start { get; }
    
    public string? Text { get; }

    public object? Value { get; }

    public int Length => Text?.Length ?? 0;

    public int End => Start + Length;

    public override IEnumerable<SyntaxNode> GetChildren() => Enumerable.Empty<SyntaxNode>();
}