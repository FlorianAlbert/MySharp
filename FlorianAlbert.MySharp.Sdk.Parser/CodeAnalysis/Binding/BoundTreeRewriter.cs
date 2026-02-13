using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal abstract class BoundTreeRewriter
{
    public virtual BoundExpression RewriteExpression(BoundExpression expression)
    {
        return expression.Kind switch
        {
            BoundNodeKind.ErrorExpression => RewriteErrorExpression((BoundErrorExpression) expression),
            BoundNodeKind.UnaryExpression => RewriteUnaryExpression((BoundUnaryExpression) expression),
            BoundNodeKind.LiteralExpression => RewriteLiteralExpression((BoundLiteralExpression) expression),
            BoundNodeKind.BinaryExpression => RewriteBinaryExpression((BoundBinaryExpression) expression),
            BoundNodeKind.VariableExpression => RewriteVariableExpression((BoundVariableExpression) expression),
            BoundNodeKind.AssignmentExpression => RewriteAssignmentExpression((BoundAssignmentExpression) expression),
            BoundNodeKind.CallExpression => RewriteCallExpression((BoundCallExpression) expression),
            _ => throw new InvalidOperationException($"Unexpected expression kind: {expression.Kind}"),
        };
    }

    protected virtual BoundErrorExpression RewriteErrorExpression(BoundErrorExpression errorExpression)
    {
        return errorExpression;
    }

    protected virtual BoundUnaryExpression RewriteUnaryExpression(BoundUnaryExpression unaryExpression)
    {
        BoundExpression rewrittenExpression = RewriteExpression(unaryExpression.Operand);
        if (rewrittenExpression == unaryExpression.Operand)
        {
            return unaryExpression;
        }

        return new BoundUnaryExpression(unaryExpression.Operator, rewrittenExpression);
    }

    protected virtual BoundLiteralExpression RewriteLiteralExpression(BoundLiteralExpression literalExpression)
    {
        return literalExpression;
    }

    protected virtual BoundBinaryExpression RewriteBinaryExpression(BoundBinaryExpression binaryExpression)
    {
        BoundExpression rewrittenLeftExpression = RewriteExpression(binaryExpression.Left);
        BoundExpression rewrittenRightExpression = RewriteExpression(binaryExpression.Right);
        if (rewrittenLeftExpression == binaryExpression.Left && 
            rewrittenRightExpression == binaryExpression.Right)
        {
            return binaryExpression;
        }

        return new BoundBinaryExpression(rewrittenLeftExpression, binaryExpression.Operator, rewrittenRightExpression);
    }

    protected virtual BoundVariableExpression RewriteVariableExpression(BoundVariableExpression variableExpression)
    {
        return variableExpression;
    }

    protected virtual BoundAssignmentExpression RewriteAssignmentExpression(BoundAssignmentExpression assignmentExpression)
    {
        BoundExpression rewrittenExpression = RewriteExpression(assignmentExpression.Expression);
        if (rewrittenExpression == assignmentExpression.Expression)
        {
            return assignmentExpression;
        }

        return new BoundAssignmentExpression(assignmentExpression.VariableSymbol, rewrittenExpression);
    }

    protected virtual BoundExpression RewriteCallExpression(BoundCallExpression expression)
    {
        ImmutableArray<BoundExpression>.Builder? rewrittenArgumentsBuilder = null;
        for (int argumentIndex = 0; argumentIndex < expression.Arguments.Length; argumentIndex++)
        {
            BoundExpression argument = expression.Arguments[argumentIndex];
            BoundExpression rewrittenArgument = RewriteExpression(argument);
            if (rewrittenArgument != argument)
            {
                if (rewrittenArgumentsBuilder is null)
                {
                    rewrittenArgumentsBuilder = ImmutableArray.CreateBuilder<BoundExpression>(expression.Arguments.Length);
                    for (int previousArgumentIndex = 0; previousArgumentIndex < argumentIndex; previousArgumentIndex++)
                    {
                        rewrittenArgumentsBuilder.Add(expression.Arguments[previousArgumentIndex]);
                    }
                }
            }
            rewrittenArgumentsBuilder?.Add(rewrittenArgument);
        }

        if (rewrittenArgumentsBuilder is null)
        {
            return expression;
        }

        return new BoundCallExpression(expression.FunctionSymbol, rewrittenArgumentsBuilder.ToImmutable());
    }

    public virtual BoundStatement RewriteStatement(BoundStatement statement)
    {
        return statement.Kind switch
        {
            BoundNodeKind.BlockStatement => RewriteBlockStatement((BoundBlockStatement) statement),
            BoundNodeKind.VariableDeclarationStatement => RewriteVariableDeclarationStatement((BoundVariableDeclarationStatement) statement),
            BoundNodeKind.IfStatement => RewriteIfStatement((BoundIfStatement) statement),
            BoundNodeKind.WhileStatement => RewriteWhileStatement((BoundWhileStatement) statement),
            BoundNodeKind.ForStatement => RewriteForStatement((BoundForStatement) statement),
            BoundNodeKind.LabelStatement => RewriteLabelStatement((BoundLabelStatement) statement),
            BoundNodeKind.GotoStatement => RewriteGotoStatement((BoundGotoStatement) statement),
            BoundNodeKind.ConditionalGotoStatement => RewriteConditionalGotoStatement((BoundConditionalGotoStatement) statement),
            BoundNodeKind.ExpressionStatement => RewriteExpressionStatement((BoundExpressionStatement) statement),
            _ => throw new InvalidOperationException($"Unexpected statement kind: {statement.Kind}"),
        };
    }

    protected virtual BoundBlockStatement RewriteBlockStatement(BoundBlockStatement blockStatement)
    {
        ImmutableArray<BoundStatement>.Builder? rewrittenStatementsBuilder = null;
        for (int statementIndex = 0; statementIndex < blockStatement.Statements.Length; statementIndex++)
        {
            BoundStatement statement = blockStatement.Statements[statementIndex];
            BoundStatement rewrittenStatement = RewriteStatement(statement);
            if (rewrittenStatement != statement)
            {
                if (rewrittenStatementsBuilder is null)
                {
                    rewrittenStatementsBuilder = ImmutableArray.CreateBuilder<BoundStatement>(blockStatement.Statements.Length);
                    for (int previousStatementIndex = 0; previousStatementIndex < statementIndex; previousStatementIndex++)
                    {
                        rewrittenStatementsBuilder.Add(blockStatement.Statements[previousStatementIndex]);
                    }
                }
            }

            rewrittenStatementsBuilder?.Add(rewrittenStatement);
        }

        if (rewrittenStatementsBuilder is null)
        {
            return blockStatement;
        }

        return new BoundBlockStatement(rewrittenStatementsBuilder.ToImmutable());
    }

    protected virtual BoundVariableDeclarationStatement RewriteVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement)
    {
        BoundExpression rewrittenValueExpression = RewriteExpression(variableDeclarationStatement.ValueExpression);
        if (rewrittenValueExpression == variableDeclarationStatement.ValueExpression)
        {
            return variableDeclarationStatement;
        }

        return new BoundVariableDeclarationStatement(variableDeclarationStatement.Variable, rewrittenValueExpression);
    }

    protected virtual BoundStatement RewriteIfStatement(BoundIfStatement ifStatement)
    {
        BoundExpression rewrittenCondition = RewriteExpression(ifStatement.Condition);
        BoundStatement rewrittenThenStatement = RewriteStatement(ifStatement.ThenStatement);
        BoundStatement? rewrittenElseStatement = ifStatement.ElseStatement is null ? null : RewriteStatement(ifStatement.ElseStatement);

        if (rewrittenCondition == ifStatement.Condition &&
            rewrittenThenStatement == ifStatement.ThenStatement &&
            rewrittenElseStatement == ifStatement.ElseStatement)
        {
            return ifStatement;
        }

        return new BoundIfStatement(rewrittenCondition, rewrittenThenStatement, rewrittenElseStatement);
    }

    protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement whileStatement)
    {
        BoundExpression rewrittenCondition = RewriteExpression(whileStatement.Condition);
        BoundStatement rewrittenBody = RewriteStatement(whileStatement.Body);

        if (rewrittenCondition == whileStatement.Condition &&
            rewrittenBody == whileStatement.Body)
        {
            return whileStatement;
        }

        return new BoundWhileStatement(rewrittenCondition, rewrittenBody);
    }

    protected virtual BoundStatement RewriteForStatement(BoundForStatement forStatement)
    {
        BoundExpression rewrittenLowerBound = RewriteExpression(forStatement.LowerBound);
        BoundExpression rewrittenUpperBound = RewriteExpression(forStatement.UpperBound);
        BoundStatement rewrittenBody = RewriteStatement(forStatement.Body);

        if (rewrittenLowerBound == forStatement.LowerBound &&
            rewrittenUpperBound == forStatement.UpperBound &&
            rewrittenBody == forStatement.Body)
        {
            return forStatement;
        }

        return new BoundForStatement(forStatement.IteratorSymbol, rewrittenLowerBound, rewrittenUpperBound, rewrittenBody);
    }

    protected virtual BoundLabelStatement RewriteLabelStatement(BoundLabelStatement labelStatement)
    {
        return labelStatement;
    }

    protected virtual BoundGotoStatement RewriteGotoStatement(BoundGotoStatement gotoStatement)
    {
        return gotoStatement;
    }

    protected virtual BoundConditionalGotoStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement)
    {
        BoundExpression rewrittenCondition = RewriteExpression(conditionalGotoStatement.Condition);
        if (rewrittenCondition == conditionalGotoStatement.Condition)
        {
            return conditionalGotoStatement;
        }

        return new BoundConditionalGotoStatement(conditionalGotoStatement.LabelSymbol, rewrittenCondition, conditionalGotoStatement.JumpIf);
    }

    protected virtual BoundExpressionStatement RewriteExpressionStatement(BoundExpressionStatement expressionStatement)
    {
        BoundExpression rewrittenExpression = RewriteExpression(expressionStatement.Expression);
        if (rewrittenExpression == expressionStatement.Expression)
        {
            return expressionStatement;
        }

        return new BoundExpressionStatement(rewrittenExpression);
    }
}