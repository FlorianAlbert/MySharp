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
        textWriter.WritePunctuation("{");
        textWriter.WriteLine();
        textWriter.Indent++;
        foreach (BoundStatement statement in blockStatement.Statements)
        {
            statement.WriteTo(textWriter);
        }
        textWriter.Indent--;
        textWriter.WritePunctuation("}");
        textWriter.WriteLine();
    }

    private static void WriteVariableDeclarationStatement(BoundVariableDeclarationStatement variableDeclarationStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword(variableDeclarationStatement.Variable.IsReadOnly ? "let " : "var ");
        textWriter.WriteIdentifier(variableDeclarationStatement.Variable.Name);
        textWriter.WritePunctuation(" = ");
        variableDeclarationStatement.ValueExpression.WriteTo(textWriter);
        textWriter.WritePunctuation(";");
        textWriter.WriteLine();
    }

    private static void WriteIfStatement(BoundIfStatement ifStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword("if");
        textWriter.WritePunctuation(" (");
        ifStatement.Condition.WriteTo(textWriter);
        textWriter.WritePunctuation(") ");
        textWriter.WriteLine();
        textWriter.WriteNestedStatement(ifStatement.ThenStatement);

        if (ifStatement.ElseStatement is not null)
        {
            textWriter.WriteLine();
            textWriter.WriteKeyword("else");
            textWriter.WriteLine();
            textWriter.WriteNestedStatement(ifStatement.ElseStatement);
        }

        textWriter.WriteLine();
    }

    private static void WriteWhileStatement(BoundWhileStatement whileStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword("while");
        textWriter.WritePunctuation(" (");
        whileStatement.Condition.WriteTo(textWriter);
        textWriter.WritePunctuation(") ");
        textWriter.WriteLine();
        textWriter.WriteNestedStatement(whileStatement.Body);
        textWriter.WriteLine();
    }

    private static void WriteForStatement(BoundForStatement forStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword("for");
        textWriter.WritePunctuation(" (");
        textWriter.WriteKeyword("let ");
        textWriter.WriteIdentifier(forStatement.IteratorSymbol.Name);
        textWriter.WritePunctuation(" = ");
        forStatement.LowerBound.WriteTo(textWriter);
        textWriter.WriteKeyword(" to ");
        forStatement.UpperBound.WriteTo(textWriter);
        textWriter.WritePunctuation(") ");
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
        textWriter.WritePunctuation(":");
        textWriter.WriteLine();

        if (needsUnindentation)
        {
            textWriter.Indent++;
        }
    }

    private static void WriteGotoStatement(BoundGotoStatement gotoStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword("goto ");
        textWriter.WriteIdentifier(gotoStatement.LabelSymbol.Name);
        textWriter.WritePunctuation(";");
        textWriter.WriteLine();
    }

    private static void WriteConditionalGotoStatement(BoundConditionalGotoStatement conditionalGotoStatement, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword("goto ");
        textWriter.WriteIdentifier(conditionalGotoStatement.LabelSymbol.Name);
        textWriter.WriteKeyword(conditionalGotoStatement.JumpIf is true ? " if " : " unless ");
        conditionalGotoStatement.Condition.WriteTo(textWriter);
        textWriter.WritePunctuation(";");
        textWriter.WriteLine();
    }

    private static void WriteExpressionStatement(BoundExpressionStatement expressionStatement, IndentedTextWriter textWriter)
    {
        expressionStatement.Expression.WriteTo(textWriter);
        textWriter.WritePunctuation(";");
        textWriter.WriteLine();
    }

    private static void WriteErrorExpression(BoundErrorExpression errorExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteKeyword("<error>");
    }

    private static void WriteUnaryExpression(BoundUnaryExpression unaryExpression, IndentedTextWriter textWriter)
    {
        textWriter.WritePunctuation(SyntaxFacts.GetText(unaryExpression.Operator.SyntaxKind) ?? throw new Exception("Unknown unary operator."));
        textWriter.WriteNestedExpression(SyntaxFacts.GetUnaryOperatorPrecedence(unaryExpression.Operator.SyntaxKind), unaryExpression.Operand);
    }

    private static void WriteLiteralExpression(BoundLiteralExpression literalExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteLiteral(literalExpression.Value, literalExpression.Type);
    }

    private static void WriteBinaryExpression(BoundBinaryExpression binaryExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteNestedExpression(SyntaxFacts.GetBinaryOperatorPrecedence(binaryExpression.Operator.SyntaxKind), binaryExpression.Left);
        textWriter.WritePunctuation($" {SyntaxFacts.GetText(binaryExpression.Operator.SyntaxKind) ?? throw new Exception("Unknown binary operator.")} ");
        textWriter.WriteNestedExpression(SyntaxFacts.GetBinaryOperatorPrecedence(binaryExpression.Operator.SyntaxKind), binaryExpression.Right);
    }

    private static void WriteVariableExpression(BoundVariableExpression variableExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteIdentifier(variableExpression.VariableSymbol.Name);
    }

    private static void WriteAssignmentExpression(BoundAssignmentExpression assignmentExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteIdentifier(assignmentExpression.VariableSymbol.Name);
        textWriter.WritePunctuation(" = ");
        assignmentExpression.Expression.WriteTo(textWriter);
    }

    private static void WriteCallExpression(BoundCallExpression callExpression, IndentedTextWriter textWriter)
    {
        textWriter.WriteIdentifier(callExpression.FunctionSymbol.Name);
        textWriter.WritePunctuation("(");

        for (int argumentsIndex = 0; argumentsIndex < callExpression.Arguments.Length; argumentsIndex++)
        {
            if (argumentsIndex > 0)
            {
                textWriter.WritePunctuation(", ");
            }

            callExpression.Arguments[argumentsIndex].WriteTo(textWriter);
        }

        textWriter.WritePunctuation(")");
    }

    private static void WriteConversionExpression(BoundConversionExpression conversionExpression, IndentedTextWriter textWriter)
    {
        textWriter.WritePunctuation("(");
        textWriter.WriteIdentifier(conversionExpression.Type.Name);
        textWriter.WritePunctuation(") ");
        conversionExpression.Expression.WriteTo(textWriter);
    }
}
