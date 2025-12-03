namespace FlorianAlbert.MySharp.Sdk.Parser.Binding;

public sealed class BoundBinaryExpression : BoundExpression
{
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator @operator, BoundExpression right)
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }

    public override Type Type => Operator.ResultType;
    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;

    public BoundExpression Left { get; }
    public BoundBinaryOperator Operator { get; }
    public BoundExpression Right { get; }
}