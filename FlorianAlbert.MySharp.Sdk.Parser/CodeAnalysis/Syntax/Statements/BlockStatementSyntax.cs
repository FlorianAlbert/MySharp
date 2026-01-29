using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;

public sealed class BlockStatementSyntax : StatementSyntax
{
    public BlockStatementSyntax(SyntaxToken openBraceToken, ImmutableArray<StatementSyntax> statements, SyntaxToken closeBraceToken)
    {
        OpenBraceToken = openBraceToken;
        Statements = statements;
        CloseBraceToken = closeBraceToken;
    }

    public override SyntaxKind Kind => SyntaxKind.BlockStatement;

    public override TextSpan Span => TextSpan.FromBounds(OpenBraceToken.Span.Start, CloseBraceToken.Span.End);

    public SyntaxToken OpenBraceToken { get; }

    public ImmutableArray<StatementSyntax> Statements { get; }

    public SyntaxToken CloseBraceToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return OpenBraceToken;
        foreach (StatementSyntax statement in Statements)
        {
            yield return statement;
        }

        yield return CloseBraceToken;
    }
}
