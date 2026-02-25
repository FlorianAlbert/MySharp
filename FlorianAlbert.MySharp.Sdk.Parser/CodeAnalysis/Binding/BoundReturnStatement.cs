
namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundReturnStatement : BoundStatement
{
    public BoundReturnStatement(BoundExpression? expression)
    {
        Expression = expression;
    }

    public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;

    public BoundExpression? Expression { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        if (Expression is not null)
        {
            yield return Expression;
        }
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield break;
    }
}
