using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public sealed class ElseClauseSyntax : SyntaxNode
{
    public ElseClauseSyntax(SyntaxToken elseKeyword, StatementSyntax elseStatement)
    {
        ElseKeyword = elseKeyword;
        ElseStatement = elseStatement;
    }

    public override SyntaxKind Kind => SyntaxKind.ElseClause;

    public override TextSpan Span => TextSpan.FromBounds(ElseKeyword.Span.Start, ElseStatement.Span.End);

    public SyntaxToken ElseKeyword { get; }

    public StatementSyntax ElseStatement { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return ElseKeyword;
        yield return ElseStatement;
    }
}
