using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public sealed class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(ExpressionSyntax expression, SyntaxToken endOfFileToken)
    {
        Expression = expression;
        EndOfFileToken = endOfFileToken;
    }

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

    public override TextSpan Span => TextSpan.FromBounds(Expression.Span.Start, EndOfFileToken.Span.End);

    public ExpressionSyntax Expression { get; }

    public SyntaxToken EndOfFileToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Expression;
        yield return EndOfFileToken;
    }
}
