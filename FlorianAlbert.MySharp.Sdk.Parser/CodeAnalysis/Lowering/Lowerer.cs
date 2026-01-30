using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    private Lowerer()
    {
    }

    public static BoundStatement Lower(BoundStatement statement)
    {
        Lowerer lowerer = new();
        return lowerer.RewriteStatement(statement);
    }

    protected override BoundStatement RewriteForStatement(BoundForStatement forStatement)
    {
        BoundVariableDeclarationStatement variableDeclarationStatement = new(
            forStatement.IteratorSymbol,
            forStatement.LowerBound);

        BoundBinaryExpression conditionExpression = new(
            new BoundVariableExpression(forStatement.IteratorSymbol),
            BoundBinaryOperator.Bind(SyntaxKind.LessToken, typeof(int), typeof(int))!,
            forStatement.UpperBound);

        BoundExpressionStatement incrementStatement = new(
            new BoundAssignmentExpression(
                forStatement.IteratorSymbol,
                new BoundBinaryExpression(
                    new BoundVariableExpression(forStatement.IteratorSymbol),
                    BoundBinaryOperator.Bind(SyntaxKind.PlusToken, forStatement.IteratorSymbol.Type, typeof(int))!,
                    new BoundLiteralExpression(1))));

        BoundBlockStatement whileBlockStatement = new([forStatement.Body, incrementStatement]);

        BoundWhileStatement whileStatement = new(conditionExpression, whileBlockStatement);

        BoundBlockStatement result = new([variableDeclarationStatement, whileStatement]);

        return RewriteStatement(result);
    }
}
