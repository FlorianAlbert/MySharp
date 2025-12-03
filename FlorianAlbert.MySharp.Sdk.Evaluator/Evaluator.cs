using FlorianAlbert.MySharp.Sdk.Parser.Binding;

namespace FlorianAlbert.MySharp.Sdk.Evaluator;

public sealed class Evaluator
{
    private readonly BoundExpression _root;

    public Evaluator(BoundExpression root)
    {
        _root = root;
    }

    public object? Evaluate()
    {
        return EvaluateExpression(_root);
    }

    private object? EvaluateExpression(BoundExpression expression)
    {
        if (expression is BoundLiteralExpression boundLiteralExpression &&
            boundLiteralExpression.Value is not null)
        {
            return boundLiteralExpression.Value;
        }

        if (expression is BoundUnaryExpression boundUnaryExpression)
        {
            object? operand = EvaluateExpression(boundUnaryExpression.Operand);

            return boundUnaryExpression.Operator.Kind switch
            {
                BoundUnaryOperatorKind.Identity => operand,
                BoundUnaryOperatorKind.Negation => -(int?) operand,
                BoundUnaryOperatorKind.LogicalNegation => !(bool?) operand,
                _ => throw new Exception($"Unexpected unary operator {boundUnaryExpression.Operator.Kind}"),
            };
        }

        if (expression is BoundBinaryExpression boundBinaryExpression)
        {
            object? leftValue = EvaluateExpression(boundBinaryExpression.Left);
            object? rightValue = EvaluateExpression(boundBinaryExpression.Right);

#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (boundBinaryExpression.Operator.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                    return (int?) leftValue + (int?) rightValue;
                case BoundBinaryOperatorKind.Subtraction:
                    return (int?) leftValue - (int?) rightValue;
                case BoundBinaryOperatorKind.Multiplication:
                    return (int?) leftValue * (int?) rightValue;
                case BoundBinaryOperatorKind.Division:
                    return (int?) leftValue / (int?) rightValue;
                case BoundBinaryOperatorKind.Module:
                    return (int?) leftValue % (int?) rightValue;
                case BoundBinaryOperatorKind.LogicalAnd:
                    return (bool) leftValue! && (bool) rightValue!;
                case BoundBinaryOperatorKind.LogicalOr:
                    return (bool) leftValue! || (bool) rightValue!;
                case BoundBinaryOperatorKind.BitwiseExclusiveOr:
                    return EvaluateBitwiseExclusiveOr(leftValue, rightValue);
                case BoundBinaryOperatorKind.Equals:
                    return Equals(leftValue, rightValue);
                case BoundBinaryOperatorKind.NotEquals:
                    return !Equals(leftValue, rightValue);
                default:
                    throw new Exception($"Unexpected binary operator {boundBinaryExpression.Operator.Kind}");
            }
#pragma warning restore IDE0066 // Convert switch statement to expression
        }

        throw new Exception($"Unexpected node {expression.Kind}");
    }

    private static object? EvaluateBitwiseExclusiveOr(object? leftValue, object? rightValue)
    {
#pragma warning disable IDE0038 // Use pattern matching
        if (leftValue is int && rightValue is int)
        {
            return (int) leftValue ^ (int) rightValue;
        }
#pragma warning restore IDE0038 // Use pattern matching

        return (bool) leftValue! ^ (bool) rightValue!;
    }
}
