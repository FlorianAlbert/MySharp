using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;

public sealed class VariableDeclarationStatementSyntax : StatementSyntax
{
    public VariableDeclarationStatementSyntax(SyntaxToken keywordToken, 
        SyntaxToken identifierToken, 
        SyntaxToken equalsToken, 
        ExpressionSyntax valueExpression, 
        SyntaxToken semicolonToken)
    {
        KeywordToken = keywordToken;
        IdentifierToken = identifierToken;
        EqualsToken = equalsToken;
        ValueExpression = valueExpression;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.VariableDeclarationStatement;

    public override TextSpan Span => TextSpan.FromBounds(KeywordToken.Span.Start, SemicolonToken.Span.End);

    public SyntaxToken KeywordToken { get; }

    public SyntaxToken IdentifierToken { get; }

    public SyntaxToken EqualsToken { get; }

    public ExpressionSyntax ValueExpression { get; }

    public SyntaxToken SemicolonToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return KeywordToken;
        yield return IdentifierToken;
        yield return EqualsToken;
        yield return ValueExpression;
        yield return SemicolonToken;
    }
}
