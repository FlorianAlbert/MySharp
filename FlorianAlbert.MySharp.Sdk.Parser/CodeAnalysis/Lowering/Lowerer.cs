using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    private int _variableCounter;
    private int _labelCounter;

    private Lowerer()
    {
    }

    private VariableSymbol GenerateVariableSymbol(bool isReadonly, Type type)
    {
        return new VariableSymbol($"<Variable{_variableCounter++}>", isReadonly, type);
    }

    private BoundLabel GenerateLabelSymbol()
    {
        return new BoundLabel($"<Label{_labelCounter++}>");
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
        BoundLabel endLabel = GenerateLabelSymbol();
        BoundLabelStatement endLabelStatement = new(endLabel);

        BoundBlockStatement result;
        if (ifStatement.ElseStatement is null)
        {
            BoundConditionalGotoStatement falseGotoStatement = new(
                endLabel,
                ifStatement.Condition,
                jumpIf: false);

            result = new([falseGotoStatement, ifStatement.ThenStatement, endLabelStatement]);
        }
        else
        {

            BoundLabel elseLabel = GenerateLabelSymbol();
            BoundLabelStatement elseLabelStatement = new(elseLabel);

            BoundConditionalGotoStatement elseGotoStatement = new(
                elseLabel,
                ifStatement.Condition,
                jumpIf: false);

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
        BoundLabel beginLabel = GenerateLabelSymbol();
        BoundLabelStatement beginLabelStatement = new(beginLabel);

        BoundLabel endLabel = GenerateLabelSymbol();
        BoundLabelStatement endLabelStatement = new(endLabel);

        BoundConditionalGotoStatement falseGotoStatement = new(
            endLabel,
            whileStatement.Condition,
            jumpIf: false);

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

        VariableSymbol upperBoundSymbol = GenerateVariableSymbol(isReadonly: true, typeof(int));

        BoundVariableDeclarationStatement upperBoundVariableDeclarationStatement =
            new(upperBoundSymbol, forStatement.UpperBound);

        BoundVariableExpression upperBoundExpression = new(upperBoundSymbol);

        BoundBinaryExpression conditionExpression = new(
            new BoundVariableExpression(forStatement.IteratorSymbol),
            BoundBinaryOperator.Bind(SyntaxKind.LessToken, typeof(int), typeof(int))!,
            upperBoundExpression);

        BoundExpressionStatement incrementStatement = new(
            new BoundAssignmentExpression(
                forStatement.IteratorSymbol,
                new BoundBinaryExpression(
                    new BoundVariableExpression(forStatement.IteratorSymbol),
                    BoundBinaryOperator.Bind(SyntaxKind.PlusToken, forStatement.IteratorSymbol.Type, typeof(int))!,
                    new BoundLiteralExpression(1))));

        BoundBlockStatement whileBlockStatement = new([forStatement.Body, incrementStatement]);

        BoundWhileStatement whileStatement = new(conditionExpression, whileBlockStatement);

        BoundBlockStatement result = new([variableDeclarationStatement, upperBoundVariableDeclarationStatement, whileStatement]);

        return RewriteStatement(result);
    }
}
