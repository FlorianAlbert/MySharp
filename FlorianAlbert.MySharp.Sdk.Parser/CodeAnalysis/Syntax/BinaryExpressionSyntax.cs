namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public sealed class BinaryExpressionSyntax : ExpressionSyntax
{
    public BinaryExpressionSyntax(ExpressionSyntax leftExpression, SyntaxToken operatorToken, ExpressionSyntax rightExpression)
    {
        LeftExpression = leftExpression;
        OperatorToken = operatorToken;
        RightExpression = rightExpression;
    }

    public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

    public override TextSpan Span => TextSpan.FromBounds(LeftExpression.Span.Start, RightExpression.Span.End);

    public ExpressionSyntax LeftExpression { get; }

    public SyntaxToken OperatorToken { get; }
    
    public ExpressionSyntax RightExpression { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return LeftExpression;
        yield return OperatorToken;
        yield return RightExpression;
    }
}
