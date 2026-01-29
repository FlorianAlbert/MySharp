using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public sealed class WhileStatementSyntax : StatementSyntax
{
    public WhileStatementSyntax(
        SyntaxToken whileKeyword,
        SyntaxToken openParenthesesToken,
        ExpressionSyntax conditionExpression,
        SyntaxToken closeParenthesesToken,
        StatementSyntax bodyStatement)
    {
        WhileKeyword = whileKeyword;
        OpenParenthesesToken = openParenthesesToken;
        ConditionExpression = conditionExpression;
        CloseParenthesesToken = closeParenthesesToken;
        BodyStatement = bodyStatement;
    }

    public override SyntaxKind Kind => SyntaxKind.WhileStatement;

    public override TextSpan Span => TextSpan.FromBounds(WhileKeyword.Span.Start, BodyStatement.Span.End);

    public SyntaxToken WhileKeyword { get; }

    public SyntaxToken OpenParenthesesToken { get; }

    public ExpressionSyntax ConditionExpression { get; }

    public SyntaxToken CloseParenthesesToken { get; }

    public StatementSyntax BodyStatement { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return WhileKeyword;
        yield return OpenParenthesesToken;
        yield return ConditionExpression;
        yield return CloseParenthesesToken;
        yield return BodyStatement;
    }
}
