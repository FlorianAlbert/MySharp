using FlorianAlbert.MySharp.Sdk.Parser.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.Binding;

internal sealed class Binder
{
    private readonly Dictionary<string, object?> _variables;

    public Binder(Dictionary<string, object?> variables)
    {
        _variables = variables;
    }

    public DiagnosticBag Diagnostics { get; } = [];

    public BoundExpression BindExpression(ExpressionSyntax expressionSyntax)
    {
        return expressionSyntax.Kind switch
        {
            SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpressionSyntax) expressionSyntax),
            SyntaxKind.UnaryExpression => BindUnaryExpression((UnaryExpressionSyntax) expressionSyntax),
            SyntaxKind.BinaryExpression => BindBinaryExpression((BinaryExpressionSyntax) expressionSyntax),
            SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression((ParenthesizedExpressionSyntax) expressionSyntax),
            SyntaxKind.NameExpression => BindNameExpression((NameExpressionSyntax) expressionSyntax),
            SyntaxKind.AssignmentExpression => BindAssignmentExpression((AssignmentExpressionSyntax) expressionSyntax),
            _ => throw new Exception($"Unexpected syntax {expressionSyntax.Kind}"),
        };
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax expressionSyntax)
    {
        BoundExpression boundExpression = BindExpression(expressionSyntax.Expression);

        string? name = expressionSyntax.IdentifierToken.Text;
        ArgumentNullException.ThrowIfNull(name);

        _variables[name] = null;

        return new BoundAssignmentExpression(name, boundExpression);
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax expressionSyntax)
    {
        string? name = expressionSyntax.IdentifierToken.Text;
        ArgumentNullException.ThrowIfNull(name);

        if (!_variables.TryGetValue(name, out _))
        {
            Diagnostics.ReportUndefinedName(expressionSyntax.IdentifierToken.Span, name);
            return new BoundLiteralExpression(0);
        }

        Type type = typeof(int);

        return new BoundVariableExpression(name, type);
    }

    private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax expressionSyntax) => BindExpression(expressionSyntax.ExpressionSyntax);

    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax expressionSyntax)
    {
        BoundExpression boundLeftExpression = BindExpression(expressionSyntax.LeftExpression);
        BoundExpression boundRightExpression = BindExpression(expressionSyntax.RightExpression);
        BoundBinaryOperator? boundBinaryOperator = BoundBinaryOperator.Bind(expressionSyntax.OperatorToken.Kind, boundLeftExpression.Type, boundRightExpression.Type);

        if (boundBinaryOperator is null)
        {
            Diagnostics.ReportUndefindedBinaryOperator(expressionSyntax.OperatorToken.Span, expressionSyntax.OperatorToken.Text, boundLeftExpression.Type, boundRightExpression.Type);
            return boundLeftExpression;
        }

        return new BoundBinaryExpression(boundLeftExpression, boundBinaryOperator, boundRightExpression);
    }

    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax expressionSyntax)
    {
        BoundExpression boundOperandExpression = BindExpression(expressionSyntax.Operand);
        BoundUnaryOperator? boundUnaryOperator = BoundUnaryOperator.Bind(expressionSyntax.OperatorToken.Kind, boundOperandExpression.Type);

        if (boundUnaryOperator is null)
        {
            Diagnostics.ReportUndefindedUnaryOperator(expressionSyntax.OperatorToken.Span, expressionSyntax.OperatorToken.Text, boundOperandExpression.Type);
            return boundOperandExpression;
        }

        return new BoundUnaryExpression(boundUnaryOperator, boundOperandExpression);
    }

    private BoundLiteralExpression BindLiteralExpression(LiteralExpressionSyntax expressionSyntax)
    {
        object? value = expressionSyntax.Value ?? 0;

        return new BoundLiteralExpression(value);
    }
}
