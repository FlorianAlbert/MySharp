namespace FlorianAlbert.MySharp.Sdk.Parser.Syntax;

public sealed class UnaryExpressionSyntax : ExpressionSyntax
{
    public UnaryExpressionSyntax(SyntaxToken operatorToken, ExpressionSyntax operand)
    {
        OperatorToken = operatorToken;
        Operand = operand;

        Start = operatorToken.Start;
        Length = operand.Start + operand.Length - Start;
    }

    public override SyntaxKind Kind => SyntaxKind.UnaryExpression;

    public SyntaxToken OperatorToken { get; }
    
    public ExpressionSyntax Operand { get; }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        yield return OperatorToken;
        yield return Operand;
    }
}
