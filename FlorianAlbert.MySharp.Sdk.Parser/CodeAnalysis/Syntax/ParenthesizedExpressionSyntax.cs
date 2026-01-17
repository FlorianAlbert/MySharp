using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
    public ParenthesizedExpressionSyntax(SyntaxToken openParenthesisToken, ExpressionSyntax expressionSyntax, SyntaxToken closeParenthesisToken)
    {
        OpenParenthesisToken = openParenthesisToken;
        ExpressionSyntax = expressionSyntax;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

    public override TextSpan Span => TextSpan.FromBounds(OpenParenthesisToken.Span.Start, CloseParenthesisToken.Span.End);

    public SyntaxToken OpenParenthesisToken { get; }

    public ExpressionSyntax ExpressionSyntax { get; }
    
    public SyntaxToken CloseParenthesisToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return OpenParenthesisToken;
        yield return ExpressionSyntax;
        yield return CloseParenthesisToken;
    }
}
