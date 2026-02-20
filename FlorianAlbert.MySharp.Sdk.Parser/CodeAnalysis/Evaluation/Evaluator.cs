using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Evaluation;

internal sealed class Evaluator
{
    private readonly BoundBlockStatement _root;
    private readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> _functions;
    private readonly Dictionary<VariableSymbol, object?> _globalVariables;

    private readonly Stack<StackFrame> _stackFrames = [];
    private StackFrame _CurrentStackFrame => _stackFrames.Peek();

    private object? _lastValue;

    public Evaluator(BoundBlockStatement root, ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functions, Dictionary<VariableSymbol, object?> variables)
    {
        _root = root;
        _functions = functions;
        _globalVariables = variables;
    }

    public object? Evaluate()
    {
        return EvaluateFunctionBody(_root, _globalVariables);
    }

    private object? EvaluateFunctionBody(BoundBlockStatement statement, Dictionary<VariableSymbol, object?>? initialVariables = null)
    {
        // We need to index labels first, to be able to jump
        // to labels that are declared after a goto statement.
        Dictionary<BoundLabel, int> indexedLabels = IndexFunction(statement);

        StackFrame newFrame = new(indexedLabels, initialVariables);
        _stackFrames.Push(newFrame);

        while (newFrame.CurrentStatementIndex < statement.Statements.Length)
        {
            BoundStatement nextStatement = statement.Statements[newFrame.CurrentStatementIndex];
            EvaluateStatement(nextStatement);
        }

        _stackFrames.Pop();

        return _lastValue;
    }

    private static Dictionary<BoundLabel, int> IndexFunction(BoundBlockStatement statement)
    {
        Dictionary<BoundLabel, int> indexedLabels = [];
        for (int statementIndex = 0; statementIndex < statement.Statements.Length; statementIndex++)
        {
            if (statement.Statements[statementIndex] is BoundLabelStatement labelStatement)
            {
                indexedLabels[labelStatement.LabelSymbol] = statementIndex + 1;
            }
        }

        return indexedLabels;
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
        _CurrentStackFrame.Variables[variableDeclarationStatement.Variable] = value;
        _lastValue = value;

        _CurrentStackFrame.CurrentStatementIndex++;
    }

    private void EvaluateLabelStatement()
    {
        // Labels are already indexed before, so we can just skip them here.
        _CurrentStackFrame.CurrentStatementIndex++;
    }

    private void EvaluateGotoStatement(BoundGotoStatement statement)
    {
        _CurrentStackFrame.CurrentStatementIndex = _CurrentStackFrame.IndexedLabels[statement.LabelSymbol];
    }

    private void EvaluateConditionalGotoStatement(BoundConditionalGotoStatement statement)
    {
        bool conditionValue = (bool) EvaluateExpression(statement.Condition)!;
        if (conditionValue == statement.JumpIf)
        {
            _CurrentStackFrame.CurrentStatementIndex = _CurrentStackFrame.IndexedLabels[statement.LabelSymbol];
        }
        else
        {
            _CurrentStackFrame.CurrentStatementIndex++;
        }
    }

    private void EvaluateExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        _lastValue = EvaluateExpression(expressionStatement.Expression);

        _CurrentStackFrame.CurrentStatementIndex++;
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
            BoundNodeKind.ConversionExpression => EvaluateConversionExpression((BoundConversionExpression) expression),
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

        Dictionary<VariableSymbol, object?> localVariables = _CurrentStackFrame.Variables;
        if (localVariables.ContainsKey(boundAssignmentExpression.VariableSymbol))
        {
            localVariables[boundAssignmentExpression.VariableSymbol] = value;
        }
        else
        {
            _globalVariables[boundAssignmentExpression.VariableSymbol] = value;
        }

        return value;
    }

    private object? EvaluateVariableExpression(BoundVariableExpression boundVariableExpression)
    {
        Dictionary<VariableSymbol, object?> localVariables = _CurrentStackFrame.Variables;
        if (localVariables.TryGetValue(boundVariableExpression.VariableSymbol, out object? localVariableValue))
        {
            return localVariableValue;
        }
        else
        {
            return _globalVariables[boundVariableExpression.VariableSymbol];
        }
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

    private object? EvaluateCallExpression(BoundCallExpression boundCallExpression)
    {
        if (boundCallExpression.FunctionSymbol == FunctionSymbol.BuiltIns.Print)
        {
            object? argument = EvaluateExpression(boundCallExpression.Arguments[0]);
            Console.WriteLine(argument);
            return null;
        }
        else if (boundCallExpression.FunctionSymbol == FunctionSymbol.BuiltIns.Input)
        {
            return Console.ReadLine();
        }
        else if (boundCallExpression.FunctionSymbol == FunctionSymbol.BuiltIns.Random)
        {
            int minValue = (int) EvaluateExpression(boundCallExpression.Arguments[0])!;
            int maxValue = (int) EvaluateExpression(boundCallExpression.Arguments[1])!;
            return Random.Shared.Next(minValue, maxValue);
        }
        else
        {
            Dictionary<VariableSymbol, object?> parameters = [];
            for (int argumentIndex = 0; argumentIndex < boundCallExpression.Arguments.Length; argumentIndex++)
            {
                ParameterSymbol parameterSymbol = boundCallExpression.FunctionSymbol.Parameters[argumentIndex];
                BoundExpression argumentExpression = boundCallExpression.Arguments[argumentIndex];
                object? argumentValue = EvaluateExpression(argumentExpression);

                parameters.Add(parameterSymbol, argumentValue);
            }

            BoundBlockStatement functionBody = _functions[boundCallExpression.FunctionSymbol];

            object? returnValue = EvaluateFunctionBody(functionBody, parameters);

            return returnValue;
        }
    }

    private object? EvaluateConversionExpression(BoundConversionExpression boundConversionExpression)
    {
        object? value = EvaluateExpression(boundConversionExpression.Expression);

        if (boundConversionExpression.Type == TypeSymbol.BuiltIns.Bool)
        {
            return Convert.ToBoolean(value);
        }
        else if (boundConversionExpression.Type == TypeSymbol.BuiltIns.Int32)
        {
            return Convert.ToInt32(value);
        }
        else if (boundConversionExpression.Type == TypeSymbol.BuiltIns.String)
        {
            return Convert.ToString(value);
        }
        else if (boundConversionExpression.Type == TypeSymbol.BuiltIns.Character)
        {
            return Convert.ToChar(value);
        }
        else
        {
            throw new Exception($"Unexpected type {boundConversionExpression.Type}");
        }
    }
}
