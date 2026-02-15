using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.GeneralNodes;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;
using FlorianAlbert.MySharp.Sdk.Parser.Extensions;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class Binder
{
    private BoundScope _scope;

    public Binder(BoundScope parent)
    {
        _scope = new(parent);
    }

    public DiagnosticBag Diagnostics { get; } = [];

    public static BoundGlobalScope BindGlobalScope(BoundGlobalScope? previous, CompilationUnitSyntax compilationUnitSyntax)
    {
        BoundScope parentScope = CreateParentScope(previous);
        Binder binder = new(parentScope);
        BoundStatement boundStatement = binder.BindStatement(compilationUnitSyntax.Statement);

        ImmutableArray<VariableSymbol> variables = binder._scope.GetDeclaredVariables();
        ImmutableArray<Diagnostic> diagnostics = [.. binder.Diagnostics];

        return new BoundGlobalScope(previous, diagnostics, variables, boundStatement);
    }

    private static BoundScope CreateParentScope(BoundGlobalScope? previousGlobalScope)
    {
        if (previousGlobalScope is null)
        {
            return CreateRootScope();
        }

        BoundScope parent = CreateParentScope(previousGlobalScope.Previous);

        BoundScope scope = new(parent);
        foreach (VariableSymbol variable in previousGlobalScope.Variables)
        {
            scope.TryDeclareVariable(variable);
        }

        return scope;
    }

    private static BoundScope CreateRootScope()
    {
        BoundScope rootScope = new(null);

        foreach (FunctionSymbol function in FunctionSymbol.BuiltIns.GetAll())
        {
            rootScope.TryDeclareFunction(function);
        }

        return rootScope;
    }

    private BoundStatement BindStatement(StatementSyntax statementSyntax)
    {
        return statementSyntax.Kind switch
        {
            SyntaxKind.BlockStatement => BindBlockStatement((BlockStatementSyntax) statementSyntax),
            SyntaxKind.VariableDeclarationStatement => BindVariableDeclarationStatement((VariableDeclarationStatementSyntax) statementSyntax),
            SyntaxKind.IfStatement => BindIfStatement((IfStatementSyntax) statementSyntax),
            SyntaxKind.WhileStatement => BindWhileStatement((WhileStatementSyntax) statementSyntax),
            SyntaxKind.ForStatement => BindForStatement((ForStatementSyntax) statementSyntax),
            SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatementSyntax) statementSyntax),
            _ => throw new Exception($"Unexpected syntax {statementSyntax.Kind}"),
        };
    }

    private BoundBlockStatement BindBlockStatement(BlockStatementSyntax statementSyntax)
    {
        ImmutableArray<BoundStatement>.Builder boundStatements = ImmutableArray.CreateBuilder<BoundStatement>();
        _scope = new(_scope);

        foreach (StatementSyntax statement in statementSyntax.Statements)
        {
            BoundStatement boundStatement = BindStatement(statement);
            boundStatements.Add(boundStatement);
        }

        _scope = _scope.Parent!;

        return new BoundBlockStatement(boundStatements.ToImmutable());
    }

    private BoundVariableDeclarationStatement BindVariableDeclarationStatement(VariableDeclarationStatementSyntax statementSyntax)
    {
        string name = statementSyntax.IdentifierToken.Text;
        bool isReadOnly = statementSyntax.KeywordToken.Kind == SyntaxKind.LetKeyword;

        BoundExpression boundValueExpression = BindExpression(statementSyntax.ValueExpression);

        VariableSymbol variableSymbol = new(name, isReadOnly, boundValueExpression.Type);
        if (!_scope.TryDeclareVariable(variableSymbol))
        {
            Diagnostics.ReportVariableAlreadyDeclared(statementSyntax.IdentifierToken.Span, name);
        }

        return new BoundVariableDeclarationStatement(variableSymbol, boundValueExpression);
    }

    private BoundIfStatement BindIfStatement(IfStatementSyntax statementSyntax)
    {
        BoundExpression boundConditionExpression = BindExpression(statementSyntax.ConditionExpression, TypeSymbol.Bool);

        BoundStatement boundThenStatement = BindStatement(statementSyntax.ThenStatement);

        BoundStatement? boundElseStatement = statementSyntax.ElseClause is null ? null : BindStatement(statementSyntax.ElseClause.ElseStatement);

        return new BoundIfStatement(boundConditionExpression, boundThenStatement, boundElseStatement);
    }

    private BoundWhileStatement BindWhileStatement(WhileStatementSyntax statementSyntax)
    {
        BoundExpression boundConditionExpression = BindExpression(statementSyntax.ConditionExpression, TypeSymbol.Bool);
        BoundStatement boundBodyStatement = BindStatement(statementSyntax.BodyStatement);

        return new BoundWhileStatement(boundConditionExpression, boundBodyStatement);
    }

    private BoundForStatement BindForStatement(ForStatementSyntax statementSyntax)
    {
        BoundExpression boundLowerBoundExpression = BindExpression(statementSyntax.LowerBoundExpression, TypeSymbol.Int32);
        BoundExpression boundUpperBoundExpression = BindExpression(statementSyntax.UpperBoundExpression, TypeSymbol.Int32);

        _scope = new(_scope);

        string iteratorName = statementSyntax.IdentifierToken.Text;
        VariableSymbol variableSymbol = new(iteratorName, isReadOnly: true, TypeSymbol.Int32);
        _scope.TryDeclareVariable(variableSymbol);

        BoundStatement boundBodyStatement = BindStatement(statementSyntax.Body);

        _scope = _scope.Parent!;

        return new BoundForStatement(variableSymbol, boundLowerBoundExpression, boundUpperBoundExpression, boundBodyStatement);
    }

    private BoundExpressionStatement BindExpressionStatement(ExpressionStatementSyntax statementSyntax)
    {
        BoundExpression boundExpression = BindExpression(statementSyntax.Expression, canBeVoid: true);
        return new BoundExpressionStatement(boundExpression);
    }

    private BoundExpression BindExpression(ExpressionSyntax expression, TypeSymbol expectedType)
    {
        BoundExpression boundExpression = BindExpression(expression);

        if (boundExpression.Type == TypeSymbol.Error || expectedType == TypeSymbol.Error)
        {
            return BoundErrorExpression.Instance;
        }

        if (boundExpression.Type != expectedType)
        {
            Diagnostics.ReportCannotConvert(expression.Span, boundExpression.Type, expectedType);
        }

        return boundExpression;
    }

    private BoundExpression BindExpression(ExpressionSyntax expressionSyntax, bool canBeVoid = false)
    {
        BoundExpression boundExpression = expressionSyntax.Kind switch
        {
            SyntaxKind.LiteralExpression => BindLiteralExpression((LiteralExpressionSyntax) expressionSyntax),
            SyntaxKind.UnaryExpression => BindUnaryExpression((UnaryExpressionSyntax) expressionSyntax),
            SyntaxKind.BinaryExpression => BindBinaryExpression((BinaryExpressionSyntax) expressionSyntax),
            SyntaxKind.ParenthesizedExpression => BindParenthesizedExpression((ParenthesizedExpressionSyntax) expressionSyntax),
            SyntaxKind.NameExpression => BindNameExpression((NameExpressionSyntax) expressionSyntax),
            SyntaxKind.AssignmentExpression => BindAssignmentExpression((AssignmentExpressionSyntax) expressionSyntax),
            SyntaxKind.CallExpression => BindCallExpression((CallExpressionSyntax) expressionSyntax),
            _ => throw new Exception($"Unexpected syntax {expressionSyntax.Kind}"),
        };

        if (!canBeVoid && boundExpression.Type == TypeSymbol.Void)
        {
            Diagnostics.ReportExpressionMustHaveValue(expressionSyntax.Span);
            return BoundErrorExpression.Instance;
        }

        return boundExpression;
    }

    private BoundExpression BindCallExpression(CallExpressionSyntax expressionSyntax)
    {
        ImmutableArray<BoundExpression>.Builder boundArgumentExpressions = ImmutableArray.CreateBuilder<BoundExpression>();
        foreach (ExpressionSyntax parameterSyntax in expressionSyntax.Parameters)
        {
            BoundExpression boundParameterExpression = BindExpression(parameterSyntax);
            boundArgumentExpressions.Add(boundParameterExpression);
        }

        if (!_scope.TryLookupFunction(expressionSyntax.IdentifierToken.Text, out FunctionSymbol? function, out bool symbolExists))
        {
            if (symbolExists)
            {
                Diagnostics.ReportUnexpectedSymbolKind(expressionSyntax.IdentifierToken.Span, expressionSyntax.IdentifierToken.Text, SymbolKind.Function, _scope.GetSymbolKind(expressionSyntax.IdentifierToken.Text));
            }
            else
            {
                Diagnostics.ReportUndefinedFunction(expressionSyntax.IdentifierToken.Span, expressionSyntax.IdentifierToken.Text);
            }

            return BoundErrorExpression.Instance;
        }

        if (expressionSyntax.Parameters.Count != function.Parameters.Length)
        {
            Diagnostics.ReportWrongNumberOfArguments(expressionSyntax.Span, function.Name, function.Parameters.Length, expressionSyntax.Parameters.Count);
            return BoundErrorExpression.Instance;
        }

        for (int i = 0; i < expressionSyntax.Parameters.Count; i++)
        {
            BoundExpression boundArgumentExpression = boundArgumentExpressions[i];
            TypeSymbol parameterType = function.Parameters[i].Type;

            boundArgumentExpressions[i] = BindImplicitConversion(expressionSyntax.Parameters[i], boundArgumentExpression, parameterType);
        }

        return new BoundCallExpression(function, boundArgumentExpressions.ToImmutable());
    }

    private BoundExpression BindImplicitConversion(ExpressionSyntax expressionSyntax, BoundExpression boundArgumentExpression, TypeSymbol targetType)
    {
        if (boundArgumentExpression.Type == TypeSymbol.Error)
        {
            return BoundErrorExpression.Instance;
        }

        Conversion conversion = Conversion.Classify(boundArgumentExpression.Type, targetType);
        switch (conversion)
        {
            case Conversion.None:
                Diagnostics.ReportCannotConvert(expressionSyntax.Span, boundArgumentExpression.Type, targetType);
                return boundArgumentExpression;
            case Conversion.Identity:
                return boundArgumentExpression;
            case Conversion.Implicit:
                return new BoundConversionExpression(boundArgumentExpression, targetType);
            case Conversion.Explicit:
                Diagnostics.ReportExplicitConversionNeeded(expressionSyntax.Span, boundArgumentExpression.Type, targetType);
                return boundArgumentExpression;
            default:
                throw new Exception($"Unexpected conversion type: { conversion }");
        }
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax expressionSyntax)
    {
        BoundExpression boundExpression = BindExpression(expressionSyntax.Expression);

        string name = expressionSyntax.IdentifierToken.Text;

        if (!_scope.TryLookupVariable(name, out VariableSymbol? existingVariableSymbol, out bool symbolExists))
        {
            if (symbolExists)
            {
                Diagnostics.ReportUnexpectedSymbolKind(expressionSyntax.IdentifierToken.Span, expressionSyntax.IdentifierToken.Text, SymbolKind.Variable, _scope.GetSymbolKind(expressionSyntax.IdentifierToken.Text));
            }
            else
            {
                Diagnostics.ReportUndefinedVariable(expressionSyntax.IdentifierToken.Span, name);
            }

            return boundExpression;
        }

        if (existingVariableSymbol.IsReadOnly)
        {
            Diagnostics.ReportCannotAssignToReadOnlyVariable(expressionSyntax.Span, name);
            return boundExpression;
        }

        if (existingVariableSymbol.Type != boundExpression.Type)
        {
            Diagnostics.ReportCannotConvert(expressionSyntax.EqualsToken.Span, boundExpression.Type, existingVariableSymbol.Type);
            return boundExpression;
        }

        return new BoundAssignmentExpression(existingVariableSymbol, boundExpression);
    }

    private BoundExpression BindNameExpression(NameExpressionSyntax expressionSyntax)
    {
        string name = expressionSyntax.IdentifierToken.Text;

        if (name is GlobalStringConstants.ConstEmpty)
        {
            // The token got inserted by the parser due to an error.
            return BoundErrorExpression.Instance;
        }

        if (!_scope.TryLookupVariable(name, out VariableSymbol? variableSymbol, out bool symbolExists))
        {
            if (symbolExists)
            {
                Diagnostics.ReportUnexpectedSymbolKind(expressionSyntax.IdentifierToken.Span, expressionSyntax.IdentifierToken.Text, SymbolKind.Variable, _scope.GetSymbolKind(expressionSyntax.IdentifierToken.Text));
            }
            else
            {
                Diagnostics.ReportUndefinedVariable(expressionSyntax.IdentifierToken.Span, name);
            }

            return BoundErrorExpression.Instance;
        }

        return new BoundVariableExpression(variableSymbol);
    }

    private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax expressionSyntax) => BindExpression(expressionSyntax.ExpressionSyntax);

    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax expressionSyntax)
    {
        BoundExpression boundLeftExpression = BindExpression(expressionSyntax.LeftExpression);
        BoundExpression boundRightExpression = BindExpression(expressionSyntax.RightExpression);

        if (boundLeftExpression.Type == TypeSymbol.Error || boundRightExpression.Type == TypeSymbol.Error)
        {
            return BoundErrorExpression.Instance;
        }

        BoundBinaryOperator? boundBinaryOperator = BoundBinaryOperator.Bind(expressionSyntax.OperatorToken.Kind, boundLeftExpression.Type, boundRightExpression.Type);

        if (boundBinaryOperator is null)
        {
            Diagnostics.ReportUndefindedBinaryOperator(expressionSyntax.OperatorToken.Span, expressionSyntax.OperatorToken.Text, boundLeftExpression.Type, boundRightExpression.Type);
            return BoundErrorExpression.Instance;
        }

        return new BoundBinaryExpression(boundLeftExpression, boundBinaryOperator, boundRightExpression);
    }

    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax expressionSyntax)
    {
        BoundExpression boundOperandExpression = BindExpression(expressionSyntax.Operand);

        if (boundOperandExpression.Type == TypeSymbol.Error)
        {
            return boundOperandExpression;
        }

        BoundUnaryOperator? boundUnaryOperator = BoundUnaryOperator.Bind(expressionSyntax.OperatorToken.Kind, boundOperandExpression.Type);

        if (boundUnaryOperator is null)
        {
            Diagnostics.ReportUndefindedUnaryOperator(expressionSyntax.OperatorToken.Span, expressionSyntax.OperatorToken.Text, boundOperandExpression.Type);
            return BoundErrorExpression.Instance;
        }

        return new BoundUnaryExpression(boundUnaryOperator, boundOperandExpression);
    }

    private BoundLiteralExpression BindLiteralExpression(LiteralExpressionSyntax expressionSyntax)
    {
        object? value = expressionSyntax.Value ?? 0;

        return new BoundLiteralExpression(value);
    }
}
