using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;

public sealed class LiteralExpressionSyntax : ExpressionSyntax
{
    public LiteralExpressionSyntax(SyntaxToken literalToken) 
        : this(literalToken, literalToken.Value)
    {
    }

    public LiteralExpressionSyntax(SyntaxToken literalToken, object? value)
    {
        LiteralToken = literalToken;
        Value = value;
    }

    public override SyntaxKind Kind => SyntaxKind.LiteralExpression;

    public override TextSpan Span => LiteralToken.Span;

    public SyntaxToken LiteralToken { get; }

    public object? Value { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return LiteralToken;
    }
}
