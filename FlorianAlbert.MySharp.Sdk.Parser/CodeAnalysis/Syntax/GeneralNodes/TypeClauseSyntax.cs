using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.GeneralNodes;

public sealed class TypeClauseSyntax : SyntaxNode
{
    public TypeClauseSyntax(SyntaxTree syntaxTree, SyntaxToken colonToken, SyntaxToken identifierToken)
        : base(syntaxTree)
    {
        ColonToken = colonToken;
        IdentifierToken = identifierToken;
    }

    public override SyntaxKind Kind => SyntaxKind.TypeClause;

    public override TextSpan Span => TextSpan.FromBounds(ColonToken.Span.Start, IdentifierToken.Span.End);

    public SyntaxToken ColonToken { get; }

    public SyntaxToken IdentifierToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return ColonToken;
        yield return IdentifierToken;
    }
}
