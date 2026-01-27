using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(StatementSyntax statement, SyntaxToken endOfFileToken)
    {
        Statement = statement;
        EndOfFileToken = endOfFileToken;
    }

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

    public override TextSpan Span => TextSpan.FromBounds(Statement.Span.Start, EndOfFileToken.Span.End);

    public StatementSyntax Statement { get; }

    public SyntaxToken EndOfFileToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Statement;
        yield return EndOfFileToken;
    }
}
