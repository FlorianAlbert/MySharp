using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;

public sealed class NameExpressionSyntax : ExpressionSyntax
{
    public NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken)
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind => SyntaxKind.NameExpression;

    override public TextSpan Span => IdentifierToken.Span;

    public SyntaxToken IdentifierToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return IdentifierToken;
    }
}
