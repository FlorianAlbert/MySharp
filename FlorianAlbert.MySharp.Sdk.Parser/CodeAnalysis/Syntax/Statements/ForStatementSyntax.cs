using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;

public sealed class ForStatementSyntax : StatementSyntax
{
    public ForStatementSyntax(SyntaxToken forKeyword,
        SyntaxToken openParenthesisToken,
        SyntaxToken letKeyword,
        SyntaxToken identifierToken,
        SyntaxToken equalsToken,
        ExpressionSyntax lowerBoundExpression,
        SyntaxToken toKeyword,
        ExpressionSyntax upperBoundExpression,
        SyntaxToken closeParenthesisToken,
        StatementSyntax body)
    {
        ForKeyword = forKeyword;
        OpenParenthesisToken = openParenthesisToken;
        LetKeyword = letKeyword;
        IdentifierToken = identifierToken;
        EqualsToken = equalsToken;
        LowerBoundExpression = lowerBoundExpression;
        ToKeyword = toKeyword;
        UpperBoundExpression = upperBoundExpression;
        CloseParenthesisToken = closeParenthesisToken;
        Body = body;
    }

    public override SyntaxKind Kind => SyntaxKind.ForStatement;

    public override TextSpan Span => TextSpan.FromBounds(ForKeyword.Span.Start, Body.Span.End);

    public SyntaxToken ForKeyword { get; }

    public SyntaxToken OpenParenthesisToken { get; }

    public SyntaxToken LetKeyword { get; }

    public SyntaxToken IdentifierToken { get; }

    public SyntaxToken EqualsToken { get; }

    public ExpressionSyntax LowerBoundExpression { get; }

    public SyntaxToken ToKeyword { get; }

    public ExpressionSyntax UpperBoundExpression { get; }

    public SyntaxToken CloseParenthesisToken { get; }

    public StatementSyntax Body { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return ForKeyword;
        yield return OpenParenthesisToken;
        yield return LetKeyword;
        yield return IdentifierToken;
        yield return EqualsToken;
        yield return LowerBoundExpression;
        yield return ToKeyword;
        yield return UpperBoundExpression;
        yield return CloseParenthesisToken;
        yield return Body;
    }
}
