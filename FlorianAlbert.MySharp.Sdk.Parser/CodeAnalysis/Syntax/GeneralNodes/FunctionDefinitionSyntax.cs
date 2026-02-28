using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.GeneralNodes;

public sealed class FunctionDefinitionSyntax : CompilationUnitSyntaxMember
{
    public FunctionDefinitionSyntax(SyntaxTree syntaxTree,
        SyntaxToken functionKeyword,
        SyntaxToken identifierToken,
        SyntaxToken openParenthesisToken,
        SeparatedSyntaxList<ParameterSyntax> parameters,
        SyntaxToken closeParenthesisToken,
        TypeClauseSyntax? typeClause,
        BlockStatementSyntax bodyStatement)
        : base(syntaxTree)
    {
        FunctionKeyword = functionKeyword;
        IdentifierToken = identifierToken;
        OpenParenthesisToken = openParenthesisToken;
        Parameters = parameters;
        CloseParenthesisToken = closeParenthesisToken;
        TypeClause = typeClause;
        BodyStatement = bodyStatement;
    }

    public override SyntaxKind Kind => SyntaxKind.FunctionDefinition;

    public override TextSpan Span => TextSpan.FromBounds(FunctionKeyword.Span.Start, BodyStatement.Span.End);

    public SyntaxToken FunctionKeyword { get; }

    public SyntaxToken IdentifierToken { get; }

    public SyntaxToken OpenParenthesisToken { get; }

    public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

    public SyntaxToken CloseParenthesisToken { get; }

    public TypeClauseSyntax? TypeClause { get; }

    public BlockStatementSyntax BodyStatement { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return FunctionKeyword;
        yield return IdentifierToken;
        yield return OpenParenthesisToken;

        foreach (ParameterSyntax parameter in Parameters)
        {
            yield return parameter;
        }

        yield return CloseParenthesisToken;

        if (TypeClause is not null)
        {
            yield return TypeClause;
        }

        yield return BodyStatement;
    }
}
