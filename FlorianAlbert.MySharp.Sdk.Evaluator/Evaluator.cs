using FlorianAlbert.MySharp.Binding;
using FlorianAlbert.MySharp.Syntax;

namespace FlorianAlbert.MySharp;

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
            int? operand = (int?) EvaluateExpression(boundUnaryExpression.Operand);

            return boundUnaryExpression.OperatorKind switch
            {
                BoundUnaryOperatorKind.Identity => operand,
                BoundUnaryOperatorKind.Negation => -operand,
                _ => throw new Exception($"Unexpected unary operator {boundUnaryExpression.OperatorKind}"),
            };
        }

        if (expression is BoundBinaryExpression boundBinaryExpression)
        {
            int? leftValue = (int?) EvaluateExpression(boundBinaryExpression.Left);
            int? rightValue = (int?) EvaluateExpression(boundBinaryExpression.Right);

            switch (boundBinaryExpression.OperatorKind)
            {
                case BoundBinaryOperatorKind.Addition:
                    return leftValue + rightValue;
                case BoundBinaryOperatorKind.Subtraction:
                    return leftValue - rightValue;
                case BoundBinaryOperatorKind.Multiplication:
                    return leftValue * rightValue;
                case BoundBinaryOperatorKind.Division:
                    return leftValue / rightValue;
                case BoundBinaryOperatorKind.Module:
                    return leftValue % rightValue;
                default:
                    throw new Exception($"Unexpected binary operator {boundBinaryExpression.OperatorKind}");
            }
        }

        throw new Exception($"Unexpected node {expression.Kind}");
    }
}
