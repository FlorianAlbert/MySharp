using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    private int _labelCounter = 0;

    private Lowerer()
    {
    }

    private LabelSymbol GenerateLabelSymbol()
    {
        return new LabelSymbol($"<Label{_labelCounter++}>");
    }

    private static BoundBlockStatement Flatten(BoundStatement statement)
    {
        ImmutableArray<BoundStatement>.Builder statementsBuilder = ImmutableArray.CreateBuilder<BoundStatement>();

        Stack<BoundStatement> stack = [];
        stack.Push(statement);

        while (stack.Count > 0)
        {
            BoundStatement current = stack.Pop();
            if (current is BoundBlockStatement blockStatement)
            {
                for (int i = blockStatement.Statements.Length - 1; i >= 0; i--)
                {
                    stack.Push(blockStatement.Statements[i]);
                }
            }
            else
            {
                statementsBuilder.Add(current);
            }
        }

        return new BoundBlockStatement(statementsBuilder.ToImmutable());
    }

    public static BoundBlockStatement Lower(BoundStatement statement)
    {
        Lowerer lowerer = new();
        BoundStatement loweredStatement = lowerer.RewriteStatement(statement);
        return Flatten(loweredStatement);
    }

    protected override BoundStatement RewriteIfStatement(BoundIfStatement ifStatement)
    {
        LabelSymbol endLabel = GenerateLabelSymbol();
        BoundLabelStatement endLabelStatement = new(endLabel);

        BoundBlockStatement result;
        if (ifStatement.ElseStatement is null)
        {
            BoundConditionalGotoStatement falseGotoStatement = new(
                endLabel,
                ifStatement.Condition,
                jumpIfFalse: true);

            result = new([falseGotoStatement, ifStatement.ThenStatement, endLabelStatement]);
        }
        else
        {

            LabelSymbol elseLabel = GenerateLabelSymbol();
            BoundLabelStatement elseLabelStatement = new(elseLabel);

            BoundConditionalGotoStatement elseGotoStatement = new(
                elseLabel,
                ifStatement.Condition,
                jumpIfFalse: true);

            BoundGotoStatement gotoEndStatement = new(endLabel);

            result = new([
                elseGotoStatement,
                ifStatement.ThenStatement,
                gotoEndStatement,
                elseLabelStatement,
                ifStatement.ElseStatement,
                endLabelStatement
            ]);
        }

        return RewriteStatement(result);
    }

    protected override BoundStatement RewriteWhileStatement(BoundWhileStatement whileStatement)
    {
        LabelSymbol beginLabel = GenerateLabelSymbol();
        BoundLabelStatement beginLabelStatement = new(beginLabel);

        LabelSymbol endLabel = GenerateLabelSymbol();
        BoundLabelStatement endLabelStatement = new(endLabel);

        BoundConditionalGotoStatement falseGotoStatement = new(
            endLabel,
            whileStatement.Condition,
            jumpIfFalse: true);

        BoundGotoStatement gotoBeginStatement = new(beginLabel);

        BoundBlockStatement result = new([
            beginLabelStatement,
            falseGotoStatement,
            whileStatement.Body,
            gotoBeginStatement,
            endLabelStatement
        ]);

        return RewriteStatement(result);
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
