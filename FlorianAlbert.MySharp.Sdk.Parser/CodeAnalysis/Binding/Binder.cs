using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Lowering;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.GeneralNodes;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using FlorianAlbert.MySharp.Sdk.Parser.Extensions;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class Binder
{
    private FunctionSymbol? _currentFunction;

    private BoundScope _scope;
    private readonly Stack<(BoundLabel BreakLabel, BoundLabel ContinueLabel)> _loopLabels = [];
    private int _labelCounter;

    public Binder(BoundScope parent)
    {
        _scope = new(parent);
    }

    public DiagnosticBag Diagnostics { get; } = [];

    private BoundLabel GenerateLabelSymbol()
    {
        return new BoundLabel($"<Binder_Label{_labelCounter++}>");
    }

    public static BoundCompilationUnit BindCompilationUnit(BoundCompilationUnit? previous, CompilationUnitSyntax compilationUnitSyntax)
    {
        BoundScope parentScope = CreateParentScope(previous);
        Binder binder = new(parentScope);

        IEnumerable<FunctionDefinitionSyntax> functionDefinitions = compilationUnitSyntax.CompilationUnitMembers.Where(member => member.Kind is SyntaxKind.FunctionDefinition).Cast<FunctionDefinitionSyntax>();
        IEnumerable<GlobalStatementSyntax> globalStatements = compilationUnitSyntax.CompilationUnitMembers.Where(member => member.Kind is SyntaxKind.GlobalStatement).Cast<GlobalStatementSyntax>();

        // We first iterate to get to know all of the function symbols
        binder.BindFunctionDeclarations(functionDefinitions);

        BoundGlobalScope boundGlobalScope = binder.BindGlobalScope(globalStatements);
        BoundProgram boundProgram = binder.BindFunctionDefinitions(previous, functionDefinitions);

        ImmutableArray<Diagnostic> diagnostics = [.. binder.Diagnostics];

        return new BoundCompilationUnit(previous, diagnostics, boundGlobalScope, boundProgram);
    }

    private static BoundScope CreateParentScope(BoundCompilationUnit? previousCompilationUnit)
    {
        if (previousCompilationUnit is null)
        {
            return CreateRootScope();
        }

        BoundScope parent = CreateParentScope(previousCompilationUnit.Previous);

        BoundScope scope = new(parent);

        foreach (VariableSymbol variable in previousCompilationUnit.GlobalScope.Variables)
        {
            scope.TryDeclareVariable(variable);
        }

        foreach (FunctionSymbol function in previousCompilationUnit.GlobalScope.Functions)
        {
            scope.TryDeclareFunction(function);
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

        foreach (TypeSymbol type in TypeSymbol.BuiltIns.GetAll())
        {
            rootScope.TryDeclareType(type);
        }

        return rootScope;
    }

    private void BindFunctionDeclarations(IEnumerable<FunctionDefinitionSyntax> functionDefinitions)
    {
        foreach (FunctionDefinitionSyntax functionDefinitionSyntax in functionDefinitions)
        {
            BindFunctionDeclaration(functionDefinitionSyntax);
        }
    }

    private void BindFunctionDeclaration(FunctionDefinitionSyntax functionDefinitionSyntax)
    {
        string name = functionDefinitionSyntax.IdentifierToken.Text;

        ImmutableArray<ParameterSymbol>.Builder parameterSymbols = ImmutableArray.CreateBuilder<ParameterSymbol>();
        foreach (ParameterSyntax parameterSyntax in functionDefinitionSyntax.Parameters)
        {
            string parameterName = parameterSyntax.Identifier.Text;
            TypeSymbol? parameterType = BindTypeClause(parameterSyntax.TypeClause);

            if (parameterSymbols.Any(existingSymbol => existingSymbol.Name.Equals(parameterName, StringComparison.Ordinal)))
            {
                Diagnostics.ReportDuplicateParameterName(parameterSyntax.Identifier.Span, parameterName);
            }

            ParameterSymbol parameterSymbol = new(parameterName, parameterType ?? TypeSymbol.Error);
            parameterSymbols.Add(parameterSymbol);
        }

        TypeSymbol returnType = TypeSymbol.Void;
        if (functionDefinitionSyntax.TypeClause is TypeClauseSyntax typeClause)
        {
            returnType = BindTypeClause(typeClause) ?? TypeSymbol.Error;
        }

        FunctionSymbol functionSymbol = new(name, parameterSymbols.ToImmutable(), returnType);

        if (!_scope.TryDeclareFunction(functionSymbol))
        {
            Diagnostics.ReportFunctionAlreadyDeclared(functionDefinitionSyntax.IdentifierToken.Span, name);
        }
    }

    private BoundGlobalScope BindGlobalScope(IEnumerable<GlobalStatementSyntax> globalStatements)
    {
        BoundStatement boundStatement = BindGlobalBlockStatement(globalStatements);

        BoundBlockStatement loweredStatement = Lowerer.Lower(boundStatement);

        ImmutableArray<VariableSymbol> variables = _scope.GetDeclaredVariables();
        ImmutableArray<FunctionSymbol> functions = _scope.GetDeclaredFunctions();

        return new BoundGlobalScope(variables, functions, loweredStatement);
    }

    private BoundBlockStatement BindGlobalBlockStatement(IEnumerable<GlobalStatementSyntax> globalStatements)
    {
        ImmutableArray<BoundStatement>.Builder boundGlobalStatements = ImmutableArray.CreateBuilder<BoundStatement>();
        foreach (GlobalStatementSyntax globalStatement in globalStatements)
        {
            BoundStatement boundGlobalStatement = BindStatement(globalStatement.Statement);
            boundGlobalStatements.Add(boundGlobalStatement);
        }

        BoundBlockStatement globalBlockStatement = new([.. boundGlobalStatements.ToImmutable()]);

        return globalBlockStatement;
    }

    private BoundProgram BindFunctionDefinitions(BoundCompilationUnit? previous, IEnumerable<FunctionDefinitionSyntax> functionDefinitions)
    {
        ImmutableDictionary<FunctionSymbol, BoundBlockStatement>.Builder functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();

        if (previous is not null)
        {
            // We only need the function bodies of the previous
            // Compilation unit, since it already contains all
            // function bodies of its predecessor Compilation units
            functionBodies.AddRange(previous.Program.FunctionBodies);
        }
        
        foreach (FunctionDefinitionSyntax functionDefinition in functionDefinitions)
        {
            (FunctionSymbol, BoundBlockStatement)? functionBody = BindFunctionBody(functionDefinition);
            if (functionBody is (FunctionSymbol functionSymbol, BoundBlockStatement functionStatement))
            {
                BoundBlockStatement loweredFunctionBody = Lowerer.Lower(functionStatement);

                if (functionSymbol.ReturnType != TypeSymbol.Void)
                {
                    ControlFlowGraph controlFlowGraph = ControlFlowGraph.Create(loweredFunctionBody);

                    if (!controlFlowGraph.AllPathsReturn)
                    {
                        Diagnostics.ReportNotAllPathsReturn(functionDefinition.IdentifierToken.Span, functionSymbol.Name);
                    }
                }

                functionBodies.Add(functionSymbol, loweredFunctionBody);
            }
        }

        return new BoundProgram(functionBodies.ToImmutable());
    }

    private (FunctionSymbol, BoundBlockStatement)? BindFunctionBody(FunctionDefinitionSyntax functionDefinitionSyntax)
    {
        if (!_scope.TryLookupFunction(functionDefinitionSyntax.IdentifierToken.Text, out FunctionSymbol? declaredFunction, out _))
        {
            // Something went wrong when declaring the function, so we just ignore it to not produce cascading errors
            return null;
        }

        _currentFunction = declaredFunction;

        BoundBlockStatement boundBlockStatement = BindBlockStatement(functionDefinitionSyntax.BodyStatement, [.. declaredFunction.Parameters]);

        _currentFunction = null;

        return (declaredFunction, boundBlockStatement);
    }

    private BoundExpressionStatement BindErrorStatement()
    {
        return new BoundExpressionStatement(BoundErrorExpression.Instance);
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
            SyntaxKind.BreakStatement => BindBreakStatement((BreakStatementSyntax) statementSyntax),
            SyntaxKind.ContinueStatement => BindContinueStatement((ContinueStatementSyntax) statementSyntax),
            SyntaxKind.ReturnStatement => BindReturnStatement((ReturnStatementSyntax) statementSyntax),
            SyntaxKind.ExpressionStatement => BindExpressionStatement((ExpressionStatementSyntax) statementSyntax),
            _ => throw new Exception($"Unexpected syntax {statementSyntax.Kind}"),
        };
    }

    private BoundBlockStatement BindBlockStatement(BlockStatementSyntax statementSyntax, ImmutableArray<VariableSymbol>? additionalScopeVariables = null)
    {
        _scope = new(_scope);

        if (additionalScopeVariables is not null)
        {
            foreach (VariableSymbol variable in additionalScopeVariables)
            {
                _scope.TryDeclareVariable(variable);
            }
        }

        ImmutableArray<BoundStatement> boundStatements = BindStatements(statementSyntax.Statements);

        _scope = _scope.Parent!;

        return new BoundBlockStatement(boundStatements);
    }

    private ImmutableArray<BoundStatement> BindStatements(ImmutableArray<StatementSyntax> statements)
    {
        ImmutableArray<BoundStatement>.Builder boundStatements = ImmutableArray.CreateBuilder<BoundStatement>();
        foreach (StatementSyntax statement in statements)
        {
            BoundStatement boundStatement = BindStatement(statement);
            boundStatements.Add(boundStatement);
        }

        return boundStatements.ToImmutable();
    }

    private BoundVariableDeclarationStatement BindVariableDeclarationStatement(VariableDeclarationStatementSyntax statementSyntax)
    {
        string name = statementSyntax.IdentifierToken.Text;
        bool isReadOnly = statementSyntax.KeywordToken.Kind == SyntaxKind.LetKeyword;

        BoundExpression boundValueExpression = BindExpression(statementSyntax.ValueExpression);
        TypeSymbol variableType = boundValueExpression.Type;

        if (statementSyntax.TypeClause is TypeClauseSyntax typeClause)
        {
            variableType = BindTypeClause(typeClause) ?? variableType;
        }

        boundValueExpression = BindImplicitConversion(boundValueExpression, variableType, statementSyntax.ValueExpression.Span);

        VariableSymbol variableSymbol = new(name, isReadOnly, variableType);
        if (!_scope.TryDeclareVariable(variableSymbol))
        {
            Diagnostics.ReportVariableAlreadyDeclared(statementSyntax.IdentifierToken.Span, name);
        }

        return new BoundVariableDeclarationStatement(variableSymbol, boundValueExpression);
    }

    private TypeSymbol? BindTypeClause(TypeClauseSyntax typeClause)
    {
        if (!_scope.TryLookupType(typeClause.IdentifierToken.Text, out TypeSymbol? clauseType, out bool symbolExists))
        {
            if (symbolExists)
            {
                Diagnostics.ReportUnexpectedSymbolKind(typeClause.IdentifierToken.Span, typeClause.IdentifierToken.Text, SymbolKind.Type, _scope.GetSymbolKind(typeClause.IdentifierToken.Text));
            }
            else
            {
                Diagnostics.ReportUndefinedType(typeClause.IdentifierToken.Span, typeClause.IdentifierToken.Text);
            }

            return null;
        }

        return clauseType;
    }

    private BoundIfStatement BindIfStatement(IfStatementSyntax statementSyntax)
    {
        BoundExpression boundConditionExpression = BindExpression(statementSyntax.ConditionExpression, TypeSymbol.BuiltIns.Bool);

        BoundStatement boundThenStatement = BindStatement(statementSyntax.ThenStatement);

        BoundStatement? boundElseStatement = statementSyntax.ElseClause is null ? null : BindStatement(statementSyntax.ElseClause.ElseStatement);

        return new BoundIfStatement(boundConditionExpression, boundThenStatement, boundElseStatement);
    }

    private BoundWhileStatement BindWhileStatement(WhileStatementSyntax statementSyntax)
    {
        BoundExpression boundConditionExpression = BindExpression(statementSyntax.ConditionExpression, TypeSymbol.BuiltIns.Bool);
        BoundStatement boundBodyStatement = BindLoopBody(statementSyntax.BodyStatement, out BoundLabel breakLabel, out BoundLabel continueLabel);

        return new BoundWhileStatement(boundConditionExpression, boundBodyStatement, breakLabel, continueLabel);
    }

    private BoundForStatement BindForStatement(ForStatementSyntax statementSyntax)
    {
        BoundExpression boundLowerBoundExpression = BindExpression(statementSyntax.LowerBoundExpression, TypeSymbol.BuiltIns.Int32);
        BoundExpression boundUpperBoundExpression = BindExpression(statementSyntax.UpperBoundExpression, TypeSymbol.BuiltIns.Int32);

        _scope = new(_scope);

        string iteratorName = statementSyntax.IdentifierToken.Text;
        VariableSymbol variableSymbol = new(iteratorName, isReadOnly: true, TypeSymbol.BuiltIns.Int32);
        _scope.TryDeclareVariable(variableSymbol);

        BoundStatement boundBodyStatement = BindLoopBody(statementSyntax.Body, out BoundLabel breakLabel, out BoundLabel continueLabel);

        _scope = _scope.Parent!;

        return new BoundForStatement(variableSymbol, boundLowerBoundExpression, boundUpperBoundExpression, boundBodyStatement, breakLabel, continueLabel);
    }

    private BoundStatement BindLoopBody(StatementSyntax statementSyntax, out BoundLabel breakLabel, out BoundLabel continueLabel)
    {
        breakLabel = GenerateLabelSymbol();
        continueLabel = GenerateLabelSymbol();

        _loopLabels.Push((breakLabel, continueLabel));
        BoundStatement boundLoopBodyStatement = BindStatement(statementSyntax);
        _loopLabels.Pop();

        return boundLoopBodyStatement;
    }

    private BoundStatement BindBreakStatement(BreakStatementSyntax statementSyntax)
    {
        if (_loopLabels.Count <= 0)
        {
            Diagnostics.ReportBreakOrContinueOutsideOfLoop(statementSyntax.Span, statementSyntax.BreakKeyword.Text);
            return BindErrorStatement();
        }

        return new BoundGotoStatement(_loopLabels.Peek().BreakLabel);
    }

    private BoundStatement BindContinueStatement(ContinueStatementSyntax statementSyntax)
    {
        if (_loopLabels.Count <= 0)
        {
            Diagnostics.ReportBreakOrContinueOutsideOfLoop(statementSyntax.Span, statementSyntax.ContinueKeyword.Text);
            return BindErrorStatement();
        }

        return new BoundGotoStatement(_loopLabels.Peek().ContinueLabel);
    }

    private BoundStatement BindReturnStatement(ReturnStatementSyntax statementSyntax)
    {
        if (_currentFunction is null)
        {
            Diagnostics.ReportReturnOutsideOfFunction(statementSyntax.Span);
            return BindErrorStatement();
        }

        if (_currentFunction.ReturnType == TypeSymbol.Void && statementSyntax.Expression is not null)
        {
            Diagnostics.ReportReturnExpressionNotAllowed(statementSyntax.Expression.Span);
            return BindErrorStatement();
        }

        if (_currentFunction.ReturnType != TypeSymbol.Void && statementSyntax.Expression is null)
        {
            Diagnostics.ReportReturnExpressionRequired(statementSyntax.Span, _currentFunction.ReturnType);
            return BindErrorStatement();
        }

        BoundExpression? boundExpression = null;
        if (statementSyntax.Expression is ExpressionSyntax expression)
        {
            boundExpression = BindExpression(expression);
            boundExpression = BindImplicitConversion(boundExpression, _currentFunction.ReturnType, expression.Span);
        }

        return new BoundReturnStatement(boundExpression);
    }

    private BoundExpressionStatement BindExpressionStatement(ExpressionStatementSyntax statementSyntax)
    {
        BoundExpression boundExpression = BindExpression(statementSyntax.Expression, canBeVoid: true);
        return new BoundExpressionStatement(boundExpression);
    }

    private BoundExpression BindExpression(ExpressionSyntax expression, TypeSymbol expectedType)
    {
        BoundExpression boundExpression = BindExpression(expression);

        return BindImplicitConversion(boundExpression, expectedType, expression.Span);
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
            Diagnostics.ReportWrongNumberOfArguments(expressionSyntax.IdentifierToken.Span, function.Name, function.Parameters.Length, expressionSyntax.Parameters.Count);
            return BoundErrorExpression.Instance;
        }

        for (int i = 0; i < expressionSyntax.Parameters.Count; i++)
        {
            BoundExpression boundArgumentExpression = boundArgumentExpressions[i];
            TypeSymbol parameterType = function.Parameters[i].Type;

            boundArgumentExpressions[i] = BindImplicitConversion(boundArgumentExpression, parameterType, expressionSyntax.Parameters[i].Span);
        }

        return new BoundCallExpression(function, boundArgumentExpressions.ToImmutable());
    }

    private BoundExpression BindImplicitConversion(BoundExpression boundExpression, TypeSymbol targetType, TextSpan diagnosticSpan)
    {
        if (boundExpression.Type == TypeSymbol.Error || targetType == TypeSymbol.Error)
        {
            return BoundErrorExpression.Instance;
        }

        Conversion conversion = Conversion.Classify(boundExpression.Type, targetType);
        switch (conversion)
        {
            case Conversion.None:
                Diagnostics.ReportCannotConvert(diagnosticSpan, boundExpression.Type, targetType);
                return boundExpression;
            case Conversion.Identity:
                return boundExpression;
            case Conversion.Implicit:
                return new BoundConversionExpression(boundExpression, targetType);
            case Conversion.Explicit:
                Diagnostics.ReportExplicitConversionNeeded(diagnosticSpan, boundExpression.Type, targetType);
                return boundExpression;
            default:
                throw new Exception($"Unexpected conversion type: {conversion}");
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

        boundExpression = BindImplicitConversion(boundExpression, existingVariableSymbol.Type, expressionSyntax.Expression.Span);

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
