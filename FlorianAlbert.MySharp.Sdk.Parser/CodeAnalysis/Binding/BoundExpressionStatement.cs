
namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundExpressionStatement : BoundStatement
{
    public BoundExpressionStatement(BoundExpression expression)
    {
        Expression = expression;
    }

    public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;

    public BoundExpression Expression { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield return Expression;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield break;
    }
}
