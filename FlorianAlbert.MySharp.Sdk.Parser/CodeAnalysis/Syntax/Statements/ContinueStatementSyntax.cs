using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;

public sealed class ContinueStatementSyntax : StatementSyntax
{
    public ContinueStatementSyntax(SyntaxTree syntaxTree, SyntaxToken continueKeyword, SyntaxToken semicolonToken)
        : base(syntaxTree)
    {
        ContinueKeyword = continueKeyword;
        SemicolonToken = semicolonToken;
    }

    public override SyntaxKind Kind => SyntaxKind.ContinueStatement;

    public override TextSpan Span => TextSpan.FromBounds(ContinueKeyword.Span.Start, SemicolonToken.Span.End);

    public SyntaxToken ContinueKeyword { get; }

    public SyntaxToken SemicolonToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return ContinueKeyword;
        yield return SemicolonToken;
    }
}
