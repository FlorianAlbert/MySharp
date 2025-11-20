using FlorianAlbert.MySharp.Syntax;

namespace FlorianAlbert.MySharp.Binding;

public sealed class Binder
{
    private readonly List<string> _diagnostics = new();
    public IEnumerable<string> Diagnostics => _diagnostics;

    public BoundExpression BindExpression(ExpressionSyntax expressionSyntax)
    {
        return expressionSyntax.Kind switch
        {
            SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpressionSyntax) expressionSyntax),
            SyntaxKind.UnaryExpression => BindUnaryExpression((UnaryExpressionSyntax) expressionSyntax),
            SyntaxKind.BinaryExpression => BindBinaryExpression((BinaryExpressionSyntax) expressionSyntax),
            SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression((ParenthesizedExpressionSyntax) expressionSyntax),
            _ => throw new Exception($"Unexpected syntax {expressionSyntax.Kind}"),
        };
    }

    private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax expressionSyntax) => BindExpression(expressionSyntax.ExpressionSyntax);

    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax expressionSyntax)
    {
        BoundExpression boundLeftExpression = BindExpression(expressionSyntax.LeftExpression);
        BoundExpression boundRightExpression = BindExpression(expressionSyntax.RightExpression);
        BoundBinaryOperatorKind? boundBinaryOperatorKind = BindBinaryOperatorKind(expressionSyntax.OperatorToken.Kind, boundLeftExpression.Type, boundRightExpression.Type);

        if (boundBinaryOperatorKind is null)
        {
            _diagnostics.Add($"Binary operator {expressionSyntax.OperatorToken.Kind} is not defined for type {boundLeftExpression.Type} and type {boundRightExpression.Type}");
            return boundLeftExpression;
        }

        return new BoundBinaryExpression(boundLeftExpression, boundBinaryOperatorKind.Value, boundRightExpression);
    }

    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax expressionSyntax)
    {
        BoundExpression boundOperandExpression = BindExpression(expressionSyntax.Operand);
        BoundUnaryOperatorKind? boundUnaryOperatorKind = BindUnaryOperatorKind(expressionSyntax.OperatorToken.Kind, boundOperandExpression.Type);

        if (boundUnaryOperatorKind is null)
        {
            _diagnostics.Add($"Unary operator {expressionSyntax.OperatorToken.Kind} is not defined for type {boundOperandExpression.Type}");
            return boundOperandExpression;
        }

        return new BoundUnaryExpression(boundUnaryOperatorKind.Value, boundOperandExpression);
    }

    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax expressionSyntax)
    {
        object? value = expressionSyntax.Value ?? 0;

        return new BoundLiteralExpression(value);
    }

    private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type? operandType)
    {
        if (operandType != typeof(int))
        {
            return null;
        }

        return kind switch
        {
            SyntaxKind.PlusToken => BoundUnaryOperatorKind.Identity,
            SyntaxKind.MinusToken => BoundUnaryOperatorKind.Negation,
            _ => throw new Exception($"Unexpected unary operator {kind}")
        };
    }

    private BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind kind, Type? leftType, Type? rightType)
    {
        if (leftType != typeof(int) || rightType != typeof(int))
        {
            return null;
        }

        return kind switch
        {
            SyntaxKind.PlusToken => BoundBinaryOperatorKind.Addition,
            SyntaxKind.MinusToken => BoundBinaryOperatorKind.Subtraction,
            SyntaxKind.StarToken => BoundBinaryOperatorKind.Multiplication,
            SyntaxKind.SlashToken => BoundBinaryOperatorKind.Division,
            SyntaxKind.PercentToken => BoundBinaryOperatorKind.Module,
            _ => throw new Exception($"Unexpected binary operator {kind}")
        };
    }
}
