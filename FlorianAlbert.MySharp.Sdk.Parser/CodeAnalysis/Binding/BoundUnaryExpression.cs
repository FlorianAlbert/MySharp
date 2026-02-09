using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundUnaryExpression : BoundExpression
{
    public BoundUnaryExpression(BoundUnaryOperator @operator, BoundExpression operand)
    {
        Operator = @operator;
        Operand = operand;
    }

    public override TypeSymbol Type => Operator.ResultType;

    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;

    public BoundUnaryOperator Operator { get; }

    public BoundExpression Operand { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield return Operand;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(Type), Type);
    }
}
