using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;

public sealed class BreakStatementSyntax : StatementSyntax
{
    public BreakStatementSyntax(SyntaxToken breakKeyword, SyntaxToken semicolonToken)
    {
        BreakKeyword = breakKeyword;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.BreakStatement;

    public override TextSpan Span => TextSpan.FromBounds(BreakKeyword.Span.Start, SemicolonToken.Span.End);

    public SyntaxToken BreakKeyword { get; }

    public SyntaxToken SemicolonToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return BreakKeyword;
        yield return SemicolonToken;
    }
}
