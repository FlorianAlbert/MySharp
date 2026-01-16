using FlorianAlbert.MySharp.Sdk.Parser.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.Binding;

internal sealed class Binder
{
    private readonly Dictionary<VariableSymbol, object?> _variables;

    public Binder(Dictionary<VariableSymbol, object?> variables)
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

    private BoundAssignmentExpression BindAssignmentExpression(AssignmentExpressionSyntax expressionSyntax)
    {
        BoundExpression boundExpression = BindExpression(expressionSyntax.Expression);

        string? name = expressionSyntax.IdentifierToken.Text;
        ArgumentNullException.ThrowIfNull(name);

        Type? type = boundExpression.Type;
        ArgumentNullException.ThrowIfNull(type);

        VariableSymbol? existingVariable = _variables.Keys.FirstOrDefault(v => v.Name == name);
        if (existingVariable is not null)
        {
            _variables.Remove(existingVariable);
        }

        VariableSymbol variableSymbol = new(name, type);
        _variables[variableSymbol] = null;

        return new BoundAssignmentExpression(variableSymbol, boundExpression);
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax expressionSyntax)
    {
        string? name = expressionSyntax.IdentifierToken.Text;
        ArgumentNullException.ThrowIfNull(name);

        VariableSymbol? variableSymbol = _variables.Keys.FirstOrDefault(v => v.Name == name);

        if (variableSymbol is null)
        {
            Diagnostics.ReportUndefinedName(expressionSyntax.IdentifierToken.Span, name);
            return new BoundLiteralExpression(0);
        }

        return new BoundVariableExpression(variableSymbol);
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
