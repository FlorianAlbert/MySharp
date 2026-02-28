using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    public UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken operatorToken, ExpressionSyntax operand)
        : base(syntaxTree)
    {
        OperatorToken = operatorToken;
        Operand = operand;
    }

    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;

    public override TextSpan Span => TextSpan.FromBounds(OperatorToken.Span.Start, Operand.Span.End);

    public SyntaxToken OperatorToken { get; }
    
    public ExpressionSyntax Operand { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return OperatorToken;
        yield return Operand;
    }
}
