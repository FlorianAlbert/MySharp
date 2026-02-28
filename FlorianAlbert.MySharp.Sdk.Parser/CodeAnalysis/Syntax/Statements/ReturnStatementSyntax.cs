using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;

public sealed class ReturnStatementSyntax : StatementSyntax
{
    public ReturnStatementSyntax(SyntaxTree syntaxTree, SyntaxToken returnKeyword, ExpressionSyntax? expression, SyntaxToken semicolonToken)
        : base(syntaxTree)
    {
        ReturnKeyword = returnKeyword;
        Expression = expression;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

    public override TextSpan Span => TextSpan.FromBounds(ReturnKeyword.Span.Start, SemicolonToken.Span.End);

    public SyntaxToken ReturnKeyword { get; }

    public ExpressionSyntax? Expression { get; }

    public SyntaxToken SemicolonToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return ReturnKeyword;

        if (Expression is not null)
        {
            yield return Expression;
        }

        yield return SemicolonToken;
    }
}
