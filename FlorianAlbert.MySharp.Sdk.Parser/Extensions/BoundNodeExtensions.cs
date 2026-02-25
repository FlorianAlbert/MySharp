using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using System.CodeDom.Compiler;

namespace FlorianAlbert.MySharp.Sdk.Parser.Extensions;

internal static class BoundNodeExtensions
{
    extension(BoundNode boundNode)
    {
        public void WriteTo(TextWriter textWriter)
        {
            if (textWriter is IndentedTextWriter indentedTextWriter)
            {
                boundNode.WriteTo(indentedTextWriter);
            }
            else
            {
                using IndentedTextWriter newIndentedTextWriter = new(textWriter);
                boundNode.WriteTo(newIndentedTextWriter);
            }
        }

        public void WriteTo(IndentedTextWriter textWriter)
        {
            switch (boundNode.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    WriteBlockStatement((BoundBlockStatement) boundNode, textWriter);
                    break;
                case BoundNodeKind.VariableDeclarationStatement:
                    WriteVariableDeclarationStatement((BoundVariableDeclarationStatement) boundNode, textWriter);
                    break;
                case BoundNodeKind.IfStatement:
                    WriteIfStatement((BoundIfStatement) boundNode, textWriter);
                    break;
                case BoundNodeKind.WhileStatement:
                    WriteWhileStatement((BoundWhileStatement) boundNode, textWriter);
                    break;
                case BoundNodeKind.ForStatement:
                    WriteForStatement((BoundForStatement) boundNode, textWriter);
                    break;
                case BoundNodeKind.LabelStatement:
                    WriteLabelStatement((BoundLabelStatement) boundNode, textWriter);
                    break;
                case BoundNodeKind.GotoStatement:
                    WriteGotoStatement((BoundGotoStatement) boundNode, textWriter);
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    WriteConditionalGotoStatement((BoundConditionalGotoStatement) boundNode, textWriter);
                    break;
                case BoundNodeKind.ExpressionStatement:
                    WriteExpressionStatement((BoundExpressionStatement) boundNode, textWriter);
                    break;
                case BoundNodeKind.ErrorExpression:
                    WriteErrorExpression((BoundErrorExpression) boundNode, textWriter);
                    break;
                case BoundNodeKind.UnaryExpression:
                    WriteUnaryExpression((BoundUnaryExpression) boundNode, textWriter);
                    break;
                case BoundNodeKind.LiteralExpression:
                    WriteLiteralExpression((BoundLiteralExpression) boundNode, textWriter);
                    break;
                case BoundNodeKind.BinaryExpression:
                    WriteBinaryExpression((BoundBinaryExpression) boundNode, textWriter);
                    break;
                case BoundNodeKind.VariableExpression:
                    WriteVariableExpression((BoundVariableExpression) boundNode, textWriter);
                    break;
                case BoundNodeKind.AssignmentExpression:
                    WriteAssignmentExpression((BoundAssignmentExpression) boundNode, textWriter);
                    break;
                case BoundNodeKind.CallExpression:
                    WriteCallExpression((BoundCallExpression) boundNode, textWriter);
                    break;
                case BoundNodeKind.ConversionExpression:
                    WriteConversionExpression((BoundConversionExpression) boundNode, textWriter);
                    break;
                default:
                    throw new Exception($"Unexpected node {boundNode.Kind}.");
            }
        }
    }

    private static void WriteBlockStatement(BoundBlockStatement blockStatement, IndentedTextWriter textWriter)
    {
        textWriter.WritePunctuation(SyntaxKind.OpenBraceToken);
        textWriter.WriteLine();
        textWriter.Indent++;
        foreach (BoundStatement statement in blockStatement.Statements)
        {
            statement.WriteTo(textWriter);
        }
        textWriter.Indent--;
        textWriter.WritePunctuation(SyntaxKind.CloseBraceToken);
        textWriter.WriteLine();
    }

    private static void WriteVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword(variableDeclarationStatement.Variable.IsReadOnly ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword);
        textWriter.WriteSpace();
        textWriter.WriteIdentifier(variableDeclarationStatement.Variable.Name);
        textWriter.WriteSpace();
        textWriter.WritePunctuation(SyntaxKind.EqualsToken);
        textWriter.WriteSpace();
        variableDeclarationStatement.ValueExpression.WriteTo(textWriter);
        textWriter.WritePunctuation(SyntaxKind.SemicolonToken);
        textWriter.WriteLine();
    }

    private static void WriteIfStatement(BoundIfStatement ifStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword(SyntaxKind.IfKeyword);
        textWriter.WriteSpace();
        textWriter.WritePunctuation(SyntaxKind.OpenParenthesisToken);
        ifStatement.Condition.WriteTo(textWriter);
        textWriter.WritePunctuation(SyntaxKind.CloseParenthesisToken);
        textWriter.WriteLine();
        textWriter.WriteNestedStatement(ifStatement.ThenStatement);

        if (ifStatement.ElseStatement is not null)
        {
            textWriter.WriteLine();
            textWriter.WriteKeyword(SyntaxKind.ElseKeyword);
            textWriter.WriteLine();
            textWriter.WriteNestedStatement(ifStatement.ElseStatement);
        }

        textWriter.WriteLine();
    }

    private static void WriteWhileStatement(BoundWhileStatement whileStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword(SyntaxKind.WhileKeyword);
        textWriter.WriteSpace();
        textWriter.WritePunctuation(SyntaxKind.OpenParenthesisToken);
        whileStatement.Condition.WriteTo(textWriter);
        textWriter.WritePunctuation(SyntaxKind.CloseParenthesisToken);
        textWriter.WriteLine();
        textWriter.WriteNestedStatement(whileStatement.Body);
        textWriter.WriteLine();
    }

    private static void WriteForStatement(BoundForStatement forStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword(SyntaxKind.ForKeyword);
        textWriter.WriteSpace();
        textWriter.WritePunctuation(SyntaxKind.OpenParenthesisToken);
        textWriter.WriteKeyword(SyntaxKind.LetKeyword);
        textWriter.WriteSpace();
        textWriter.WriteIdentifier(forStatement.IteratorSymbol.Name);
        textWriter.WriteSpace();
        textWriter.WritePunctuation(SyntaxKind.EqualsToken);
        textWriter.WriteSpace();
        forStatement.LowerBound.WriteTo(textWriter);
        textWriter.WriteSpace();
        textWriter.WriteKeyword(SyntaxKind.ToKeyword);
        textWriter.WriteSpace();
        forStatement.UpperBound.WriteTo(textWriter);
        textWriter.WritePunctuation(SyntaxKind.CloseParenthesisToken);
        textWriter.WriteLine();
        textWriter.WriteNestedStatement(forStatement.Body);
        textWriter.WriteLine();
    }

    private static void WriteLabelStatement(BoundLabelStatement labelStatement, IndentedTextWriter textWriter)
    {
        bool needsUnindentation = textWriter.Indent > 0;
        if (needsUnindentation)
        {
            textWriter.Indent--;
        }

        textWriter.WritePunctuation(labelStatement.LabelSymbol.Name);
        textWriter.WritePunctuation(SyntaxKind.ColonToken);
        textWriter.WriteLine();

        if (needsUnindentation)
        {
            textWriter.Indent++;
        }
    }

    private static void WriteGotoStatement(BoundGotoStatement gotoStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword("goto");
        textWriter.WriteSpace();
        textWriter.WriteIdentifier(gotoStatement.LabelSymbol.Name);
        textWriter.WritePunctuation(SyntaxKind.SemicolonToken);
        textWriter.WriteLine();
    }

    private static void WriteConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword("goto");
        textWriter.WriteSpace();
        textWriter.WriteIdentifier(conditionalGotoStatement.LabelSymbol.Name);
        textWriter.WriteSpace();
        if (conditionalGotoStatement.JumpIf)
        {
            textWriter.WriteKeyword(SyntaxKind.IfKeyword);
        }
        else
        {
            textWriter.WriteKeyword("unless");
        }
        textWriter.WriteSpace();
        conditionalGotoStatement.Condition.WriteTo(textWriter);
        textWriter.WritePunctuation(SyntaxKind.SemicolonToken);
        textWriter.WriteLine();
    }

    private static void WriteExpressionStatement(BoundExpressionStatement expressionStatement, IndentedTextWriter textWriter)
    {
        expressionStatement.Expression.WriteTo(textWriter);
        textWriter.WritePunctuation(SyntaxKind.SemicolonToken);
        textWriter.WriteLine();
    }

    private static void WriteErrorExpression(BoundErrorExpression errorExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword("<error>");
    }

    private static void WriteUnaryExpression(BoundUnaryExpression unaryExpression, IndentedTextWriter textWriter)
    {
        textWriter.WritePunctuation(unaryExpression.Operator.SyntaxKind);
        textWriter.WriteNestedExpression(SyntaxFacts.GetUnaryOperatorPrecedence(unaryExpression.Operator.SyntaxKind), unaryExpression.Operand);
    }

    private static void WriteLiteralExpression(BoundLiteralExpression literalExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteLiteral(literalExpression.Value, literalExpression.Type);
    }

    private static void WriteBinaryExpression(BoundBinaryExpression binaryExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteNestedExpression(SyntaxFacts.GetBinaryOperatorPrecedence(binaryExpression.Operator.SyntaxKind), binaryExpression.Left);
        textWriter.WriteSpace();
        textWriter.WritePunctuation(binaryExpression.Operator.SyntaxKind);
        textWriter.WriteSpace();
        textWriter.WriteNestedExpression(SyntaxFacts.GetBinaryOperatorPrecedence(binaryExpression.Operator.SyntaxKind), binaryExpression.Right);
    }

    private static void WriteVariableExpression(BoundVariableExpression variableExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteIdentifier(variableExpression.VariableSymbol.Name);
    }

    private static void WriteAssignmentExpression(BoundAssignmentExpression assignmentExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteIdentifier(assignmentExpression.VariableSymbol.Name);
        textWriter.WriteSpace();
        textWriter.WritePunctuation(SyntaxKind.EqualsToken);
        textWriter.WriteSpace();
        assignmentExpression.Expression.WriteTo(textWriter);
    }

    private static void WriteCallExpression(BoundCallExpression callExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteIdentifier(callExpression.FunctionSymbol.Name);
        textWriter.WritePunctuation(SyntaxKind.OpenParenthesisToken);

        for (int argumentsIndex = 0; argumentsIndex < callExpression.Arguments.Length; argumentsIndex++)
        {
            if (argumentsIndex > 0)
            {
                textWriter.WritePunctuation(SyntaxKind.CommaToken);
                textWriter.WriteSpace();
            }

            callExpression.Arguments[argumentsIndex].WriteTo(textWriter);
        }

        textWriter.WritePunctuation(SyntaxKind.CloseParenthesisToken);
    }

    private static void WriteConversionExpression(BoundConversionExpression conversionExpression, IndentedTextWriter textWriter)
    {
        textWriter.WritePunctuation(SyntaxKind.OpenParenthesisToken);
        textWriter.WriteIdentifier(conversionExpression.Type.Name);
        textWriter.WritePunctuation(SyntaxKind.CloseParenthesisToken);
        textWriter.WriteSpace();
        conversionExpression.Expression.WriteTo(textWriter);
    }
}
