using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class Binder
{
    private BoundScope _scope;

    public Binder(BoundScope? parent)
    {
        _scope = new BoundScope(parent);
    }

    public DiagnosticBag Diagnostics { get; } = [];

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnitSyntax compilationUnitSyntax)
    {
        BoundScope? parentScope = CreateParentScope(previous);
        Binder binder = new(parentScope);
        BoundStatement boundStatement = binder.BindStatement(compilationUnitSyntax.Statement);

        ImmutableArray<VariableSymbol> variables = binder._scope.GetDeclaredVariables();
        ImmutableArray<Diagnostic> diagnostics = [.. binder.Diagnostics];

        return new BoundGlobalScope(previous, diagnostics, variables, boundStatement);
    }

    private static BoundScope? CreateParentScope(BoundGlobalScope? previousGlobalScope)
    {
        if (previousGlobalScope is null)
        {
            return null;
        }

        BoundScope? parent = CreateParentScope(previousGlobalScope.Previous);

        BoundScope scope = new(parent);
        foreach (VariableSymbol variable in previousGlobalScope.Variables)
        {
            scope.TryDeclare(variable);
        }

        return scope;
    }

    private BoundStatement BindStatement(StatementSyntax statementSyntax)
    {
        return statementSyntax.Kind switch
        {
            SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatementSyntax) statementSyntax),
            SyntaxKind.BlockStatement => BindBlockStatement((BlockStatementSyntax) statementSyntax),
            _ => throw new Exception($"Unexpected syntax {statementSyntax.Kind}"),
        };
    }

    private BoundStatement BindExpressionStatement(ExpressionStatementSyntax statementSyntax)
    {
        BoundExpression boundExpression = BindExpression(statementSyntax.Expression);
        return new BoundExpressionStatement(boundExpression);
    }

    private BoundExpression BindExpression(ExpressionSyntax expressionSyntax)
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

        Type? type = boundExpression.Type;
        ArgumentNullException.ThrowIfNull(type);

        if (!_scope.TryLookup(name, out VariableSymbol? existingVariableSymbol))
        {
            existingVariableSymbol = new(name, type);
            _scope.TryDeclare(existingVariableSymbol);
        }

        if (existingVariableSymbol.Type != type)
        {
            Diagnostics.ReportCannotConvert(expressionSyntax.EqualsToken.Span, type, existingVariableSymbol.Type);
            return boundExpression;
        }

        return new BoundAssignmentExpression(existingVariableSymbol, boundExpression);
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax expressionSyntax)
    {
        string name = expressionSyntax.IdentifierToken.Text;

        if (!_scope.TryLookup(name, out VariableSymbol? variableSymbol))
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

    private BoundStatement BindBlockStatement(BlockStatementSyntax statementSyntax)
    {
        ImmutableArray<BoundStatement>.Builder boundStatements = ImmutableArray.CreateBuilder<BoundStatement>();

        foreach (var statement in statementSyntax.Statements)
        {
            BoundStatement boundStatement = BindStatement(statement);
            boundStatements.Add(boundStatement);
        }

        return new BoundBlockStatement(boundStatements.ToImmutable());
    }
}
