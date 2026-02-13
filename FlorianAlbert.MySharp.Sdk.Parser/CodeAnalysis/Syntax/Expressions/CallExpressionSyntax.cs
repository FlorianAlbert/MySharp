using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;

public class CallExpressionSyntax : ExpressionSyntax
{
    public CallExpressionSyntax(SyntaxToken identifierToken, SyntaxToken openParenthesisToken, SeparatedSyntaxList<ExpressionSyntax> parameters, SyntaxToken closeParenthesisToken)
    {
        IdentifierToken = identifierToken;
        OpenParenthesisToken = openParenthesisToken;
        Parameters = parameters;
        CloseParenthesisToken = closeParenthesisToken;
    }

    public override SyntaxKind Kind => SyntaxKind.CallExpression;

    public override TextSpan Span => TextSpan.FromBounds(IdentifierToken.Span.Start, CloseParenthesisToken.Span.End);

    public SyntaxToken IdentifierToken { get; }

    public SyntaxToken OpenParenthesisToken { get; }

    public SeparatedSyntaxList<ExpressionSyntax> Parameters { get; }

    public SyntaxToken CloseParenthesisToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return IdentifierToken;
        yield return OpenParenthesisToken;

        foreach (SyntaxNode parametersChild in Parameters.NodesAndSeparators)
        {
            yield return parametersChild;
        }

        yield return CloseParenthesisToken;
    }
}
