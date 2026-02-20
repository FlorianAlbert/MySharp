using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.GeneralNodes;

public sealed class CompilationUnitSyntax : SyntaxNode
{
    public CompilationUnitSyntax(ImmutableArray<CompilationUnitSyntaxMember> compilationUnitMembers, SyntaxToken endOfFileToken)
    {
        CompilationUnitMembers = compilationUnitMembers;
        EndOfFileToken = endOfFileToken;
    }

    public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

    public override TextSpan Span => TextSpan.FromBounds(CompilationUnitMembers.FirstOrDefault()?.Span.Start ?? EndOfFileToken.Span.Start, EndOfFileToken.Span.End);

    public ImmutableArray<CompilationUnitSyntaxMember> CompilationUnitMembers { get; }

    public SyntaxToken EndOfFileToken { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        foreach (CompilationUnitSyntaxMember compilationUnitMember in CompilationUnitMembers)
        {
            yield return compilationUnitMember;
        }

        yield return EndOfFileToken;
    }
}
