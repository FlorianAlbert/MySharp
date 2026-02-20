using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.GeneralNodes;

public sealed class ParameterSyntax : SyntaxNode
{
    public ParameterSyntax(SyntaxToken identifier, TypeClauseSyntax typeClause)
    {
        Identifier = identifier;
        TypeClause = typeClause;
    }

    public override SyntaxKind Kind => SyntaxKind.Parameter;

    public override TextSpan Span => TextSpan.FromBounds(Identifier.Span.Start, TypeClause.Span.End);

    public SyntaxToken Identifier { get; }

    public TypeClauseSyntax TypeClause { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Identifier;
        yield return TypeClause;
    }
}