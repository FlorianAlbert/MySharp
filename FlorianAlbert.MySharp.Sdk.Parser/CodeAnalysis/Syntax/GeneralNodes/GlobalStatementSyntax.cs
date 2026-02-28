using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.GeneralNodes;

public sealed class GlobalStatementSyntax : CompilationUnitSyntaxMember
{
    public GlobalStatementSyntax(SyntaxTree syntaxTree, StatementSyntax statement)
        : base(syntaxTree)
    {
        Statement = statement;
    }

    public override SyntaxKind Kind => SyntaxKind.GlobalStatement;

    public override TextSpan Span => Statement.Span;

    public StatementSyntax Statement { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return Statement;
    }
}
