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
            case BoundNodeKind.ForStatement:
                EvaluateForStatement((BoundForStatement) statement);
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

    private void EvaluateForStatement(BoundForStatement statement)
    {
        int lowerBound = (int) EvaluateExpression(statement.LowerBound)!;
        int upperBound = (int) EvaluateExpression(statement.UpperBound)!;
        for (int i = lowerBound; i < upperBound; i++)
        {
            _variables[statement.IteratorSymbol] = i;
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
            _ => throw new Exception($"Unexpected unary operator {boundUnaryExpression.Operator.Kind}"),
        };
    }

    private object? EvaluateBinaryExpression(BoundBinaryExpression boundBinaryExpression)
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
            case BoundBinaryOperatorKind.Modulo:
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
            case BoundBinaryOperatorKind.LessThan:
                return (int?) leftValue < (int?) rightValue;
            case BoundBinaryOperatorKind.LessThanOrEquals:
                return (int?) leftValue <= (int?) rightValue;
            case BoundBinaryOperatorKind.GreaterThan:
                return (int?) leftValue > (int?) rightValue;
            case BoundBinaryOperatorKind.GreaterThanOrEquals:
                return (int?) leftValue >= (int?) rightValue;

            default:
                throw new Exception($"Unexpected binary operator {boundBinaryExpression.Operator.Kind}");
        }
#pragma warning restore IDE0066 // Convert switch statement to expression
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
