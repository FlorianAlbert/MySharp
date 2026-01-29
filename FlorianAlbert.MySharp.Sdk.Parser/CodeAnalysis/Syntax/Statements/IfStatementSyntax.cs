using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.GeneralNodes;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;

public sealed class IfStatementSyntax : StatementSyntax
{
    public IfStatementSyntax(SyntaxToken ifKeyword,
        SyntaxToken openParenthesesToken,
        ExpressionSyntax conditionExpression,
        SyntaxToken closeParenthesesToken,
        StatementSyntax thenStatement,
        ElseClauseSyntax? elseClause = null)
    {
        IfKeyword = ifKeyword;
        OpenParenthesesToken = openParenthesesToken;
        ConditionExpression = conditionExpression;
        CloseParenthesesToken = closeParenthesesToken;
        ThenStatement = thenStatement;
        ElseClause = elseClause;
    }

    public override SyntaxKind Kind => SyntaxKind.IfStatement;

    public override TextSpan Span => TextSpan.FromBounds(IfKeyword.Span.Start, ElseClause?.Span.End ?? ThenStatement.Span.End);

    public SyntaxToken IfKeyword { get; }

    public SyntaxToken OpenParenthesesToken { get; }

    public ExpressionSyntax ConditionExpression { get; }

    public SyntaxToken CloseParenthesesToken { get; }

    public StatementSyntax ThenStatement { get; }

    public ElseClauseSyntax? ElseClause { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return IfKeyword;
        yield return OpenParenthesesToken;
        yield return ConditionExpression;
        yield return CloseParenthesesToken;
        yield return ThenStatement;

        if (ElseClause is not null)
        {
            yield return ElseClause;
        }
    }
}
