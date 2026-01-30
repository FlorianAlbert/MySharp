using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

internal sealed class Evaluator
{
    private readonly BoundStatement _root;
    private readonly Dictionary<VariableSymbol, object?> _variables;

    private object? _lastValue;

    public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object?> variables)
    {
        _root = root;
        _variables = variables;
    }

    public object? Evaluate()
    {
        EvaluateStatement(_root);
        return _lastValue;
    }

    private void EvaluateStatement(BoundStatement statement)
    {
        switch (statement.Kind)
        {
            case BoundNodeKind.BlockStatement:
                EvaluateBlockStatement((BoundBlockStatement) statement);
                break;
            case BoundNodeKind.VariableDeclarationStatement:
                EvaluateVariableDeclarationStatement((BoundVariableDeclarationStatement) statement);
                break;
            case BoundNodeKind.IfStatement:
                EvaluateIfStatement((BoundIfStatement) statement);
                break;
            case BoundNodeKind.WhileStatement:
                EvaluateWhileStatement((BoundWhileStatement) statement);
                break;
            case BoundNodeKind.ExpressionStatement:
                EvaluateExpressionStatement((BoundExpressionStatement) statement);
                break;
            default:
                throw new Exception($"Unexpected node {statement.Kind}");
        }
    }

    private void EvaluateBlockStatement(BoundBlockStatement blockStatement)
    {
        foreach (BoundStatement s in blockStatement.Statements)
        {
            EvaluateStatement(s);
        }
    }

    private void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
    {
        object? value = EvaluateExpression(variableDeclarationStatement.ValueExpression);
        _variables[variableDeclarationStatement.Variable] = value;
        _lastValue = value;
    }

    private void EvaluateIfStatement(BoundIfStatement statement)
    {
        bool conditionValue = (bool) EvaluateExpression(statement.Condition)!;
        if (conditionValue)
        {
            EvaluateStatement(statement.ThenStatement);
        }
        else if (statement.ElseStatement is not null)
        {
            EvaluateStatement(statement.ElseStatement);
        }
    }

    private void EvaluateWhileStatement(BoundWhileStatement statement)
    {
        while ((bool) EvaluateExpression(statement.Condition)!)
        {
            EvaluateStatement(statement.Body);
        }
    }

    private void EvaluateExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        _lastValue = EvaluateExpression(expressionStatement.Expression);
    }

    private object? EvaluateExpression(BoundExpression expression)
    {
        return expression.Kind switch
        {
            BoundNodeKind.LiteralExpression => EvaluateLiteralExpression((BoundLiteralExpression) expression),
            BoundNodeKind.VariableExpression => EvaluateVariableExpression((BoundVariableExpression) expression),
            BoundNodeKind.AssignmentExpression => EvaluateAssignmentExpression((BoundAssignmentExpression) expression),
            BoundNodeKind.UnaryExpression => EvaluateUnaryExpression((BoundUnaryExpression) expression),
            BoundNodeKind.BinaryExpression => EvaluateBinaryExpression((BoundBinaryExpression) expression),
            _ => throw new Exception($"Unexpected node {expression.Kind}")
        };
    }

    private static object? EvaluateLiteralExpression(BoundLiteralExpression boundLiteralExpression)
    {
        return boundLiteralExpression.Value;
    }

    private object? EvaluateAssignmentExpression(BoundAssignmentExpression boundAssignmentExpression)
    {
        object? value = EvaluateExpression(boundAssignmentExpression.Expression);
        _variables[boundAssignmentExpression.VariableSymbol] = value;

        return value;
    }

    private object? EvaluateVariableExpression(BoundVariableExpression boundVariableExpression)
    {
        return _variables[boundVariableExpression.VariableSymbol];
    }

    private object? EvaluateUnaryExpression(BoundUnaryExpression boundUnaryExpression)
    {
        object? operand = EvaluateExpression(boundUnaryExpression.Operand);

        return boundUnaryExpression.Operator.Kind switch
        {
            BoundUnaryOperatorKind.Identity => operand,
            BoundUnaryOperatorKind.Negation => -(int?) operand,
            BoundUnaryOperatorKind.LogicalNegation => !(bool?) operand,
            BoundUnaryOperatorKind.BitwiseNegation => ~(int?) operand,
            _ => throw new Exception($"Unexpected unary operator {boundUnaryExpression.Operator.Kind}"),
        };
    }

    private object? EvaluateBinaryExpression(BoundBinaryExpression boundBinaryExpression)
    {
        object? leftValue = EvaluateExpression(boundBinaryExpression.Left);
        object? rightValue = EvaluateExpression(boundBinaryExpression.Right);

        return boundBinaryExpression.Operator.Kind switch
        {
            BoundBinaryOperatorKind.Addition => (int?) leftValue + (int?) rightValue,
            BoundBinaryOperatorKind.Subtraction => (int?) leftValue - (int?) rightValue,
            BoundBinaryOperatorKind.Multiplication => (int?) leftValue * (int?) rightValue,
            BoundBinaryOperatorKind.Division => (int?) leftValue / (int?) rightValue,
            BoundBinaryOperatorKind.Modulo => (int?) leftValue % (int?) rightValue,
            BoundBinaryOperatorKind.BitwiseAnd => EvaluateBitwiseAnd(leftValue, rightValue),
            BoundBinaryOperatorKind.LogicalAnd => (bool) leftValue! && (bool) rightValue!,
            BoundBinaryOperatorKind.BitwiseOr => EvaluateBitwiseOr(leftValue, rightValue),
            BoundBinaryOperatorKind.LogicalOr => (bool) leftValue! || (bool) rightValue!,
            BoundBinaryOperatorKind.BitwiseExclusiveOr => EvaluateBitwiseExclusiveOr(leftValue, rightValue),
            BoundBinaryOperatorKind.LeftShift => (int?) leftValue << (int?) rightValue,
            BoundBinaryOperatorKind.RightShift => (int?) leftValue >> (int?) rightValue,
            BoundBinaryOperatorKind.Equals => Equals(leftValue, rightValue),
            BoundBinaryOperatorKind.NotEquals => !Equals(leftValue, rightValue),
            BoundBinaryOperatorKind.LessThan => (int?) leftValue < (int?) rightValue,
            BoundBinaryOperatorKind.LessThanOrEquals => (int?) leftValue <= (int?) rightValue,
            BoundBinaryOperatorKind.GreaterThan => (int?) leftValue > (int?) rightValue,
            BoundBinaryOperatorKind.GreaterThanOrEquals => (int?) leftValue >= (int?) rightValue,
            _ => throw new Exception($"Unexpected binary operator {boundBinaryExpression.Operator.Kind}"),
        };
    }

    private object? EvaluateBitwiseAnd(object? leftValue, object? rightValue)
    {
        if (leftValue is int leftInt && rightValue is int rightInt)
        {
            return leftInt & rightInt;
        }

        return (bool) leftValue! & (bool) rightValue!;
    }

    private object? EvaluateBitwiseOr(object? leftValue, object? rightValue)
    {
        if (leftValue is int leftInt && rightValue is int rightInt)
        {
            return leftInt | rightInt;
        }

        return (bool) leftValue! | (bool) rightValue!;
    }

    private static object? EvaluateBitwiseExclusiveOr(object? leftValue, object? rightValue)
    {
        if (leftValue is int leftInt && rightValue is int rightInt)
        {
            return leftInt ^ rightInt;
        }

        return (bool) leftValue! ^ (bool) rightValue!;
    }
}
