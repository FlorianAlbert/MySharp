namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundUnaryExpression : BoundExpression
{
    public BoundUnaryExpression(BoundUnaryOperator @operator, BoundExpression operand)
    {
        Operator = @operator;
        Operand = operand;
    }

    public override Type Type => Operator.ResultType;
    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public BoundUnaryOperator Operator { get; }
    public BoundExpression Operand { get; }
}
