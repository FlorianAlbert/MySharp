using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundBinaryExpression : BoundExpression
{
    public BoundBinaryExpression(BoundExpression left, BoundBinaryOperator @operator, BoundExpression right)
    {
        Left = left;
        Operator = @operator;
        Right = right;
    }

    public override TypeSymbol Type => Operator.ResultType;
    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;

    public BoundExpression Left { get; }
    public BoundBinaryOperator Operator { get; }
    public BoundExpression Right { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield return Left;
        yield return Right;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(Type), Type);
    }
}