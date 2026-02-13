using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

internal sealed class Evaluator
{
    private readonly BoundBlockStatement _root;
    private readonly Dictionary<VariableSymbol, object?> _variables;
    readonly Dictionary<BoundLabel, int> _indexedLabels = [];

    private int _currentStatementIndex;

    private object? _lastValue;

    public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object?> variables)
    {
        _root = root;
        _variables = variables;
    }

    public object? Evaluate()
    {
        // We need to index labels first, to be able to jump
        // to labels that are declared after a goto statement.
        for (int statementIndex = 0; statementIndex < _root.Statements.Length; statementIndex++)
        {
            if (_root.Statements[statementIndex] is BoundLabelStatement labelStatement)
            {
                _indexedLabels[labelStatement.LabelSymbol] = statementIndex + 1;
            }
        }

        while (_currentStatementIndex < _root.Statements.Length)
        {
            BoundStatement statement = _root.Statements[_currentStatementIndex];
            EvaluateStatement(statement);
        }

        return _lastValue;
    }

    private void EvaluateStatement(BoundStatement statement)
    {
        switch (statement.Kind)
        {
            case BoundNodeKind.VariableDeclarationStatement:
                EvaluateVariableDeclarationStatement((BoundVariableDeclarationStatement) statement);
                break;
            case BoundNodeKind.LabelStatement:
                EvaluateLabelStatement();
                break;
            case BoundNodeKind.GotoStatement:
                EvaluateGotoStatement((BoundGotoStatement) statement);
                break;
            case BoundNodeKind.ConditionalGotoStatement:
                EvaluateConditionalGotoStatement((BoundConditionalGotoStatement) statement);
                break;
            case BoundNodeKind.ExpressionStatement:
                EvaluateExpressionStatement((BoundExpressionStatement) statement);
                break;
            default:
                throw new Exception($"Unexpected node {statement.Kind}");
        }
    }

    private void EvaluateVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
    {
        object? value = EvaluateExpression(variableDeclarationStatement.ValueExpression);
        _variables[variableDeclarationStatement.Variable] = value;
        _lastValue = value;

        _currentStatementIndex++;
    }

    private void EvaluateLabelStatement()
    {
        // Labels are already indexed before, so we can just skip them here.
        _currentStatementIndex++;
    }

    private void EvaluateGotoStatement(BoundGotoStatement statement)
    {
        _currentStatementIndex = _indexedLabels[statement.LabelSymbol];
    }

    private void EvaluateConditionalGotoStatement(BoundConditionalGotoStatement statement)
    {
        bool conditionValue = (bool) EvaluateExpression(statement.Condition)!;
        if (conditionValue == statement.JumpIf)
        {
            _currentStatementIndex = _indexedLabels[statement.LabelSymbol];
        }
        else
        {
            _currentStatementIndex++;
        }
    }

    private void EvaluateExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        _lastValue = EvaluateExpression(expressionStatement.Expression);

        _currentStatementIndex++;
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
            BoundNodeKind.CallExpression => EvaluateCallExpression((BoundCallExpression) expression),
            _ => throw new Exception($"Unexpected node {expression.Kind}")
        };
    }

    private static object EvaluateLiteralExpression(BoundLiteralExpression boundLiteralExpression)
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
            BoundBinaryOperatorKind.Concatenation => EvaluateConcatenation(leftValue, rightValue),
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

    private static string EvaluateConcatenation(object? leftValue, object? rightValue)
    {
        return $"{leftValue}{rightValue}";
    }

    private object? EvaluateCallExpression(BoundCallExpression expression)
    {
        if (expression.FunctionSymbol == FunctionSymbol.BuiltIns.Print)
        {
            object? argument = EvaluateExpression(expression.Arguments[0]);
            Console.WriteLine(argument);
            return null;
        }
        else if (expression.FunctionSymbol == FunctionSymbol.BuiltIns.Input)
        {
            return Console.ReadLine();
        }
        else if (expression.FunctionSymbol == FunctionSymbol.BuiltIns.Random)
        {
            int minValue = (int) EvaluateExpression(expression.Arguments[0])!;
            int maxValue = (int) EvaluateExpression(expression.Arguments[1])!;
            return Random.Shared.Next(minValue, maxValue);
        }
        else
        {
            throw new Exception($"Unexpected function {expression.FunctionSymbol.Name}");
        }
    }
}
