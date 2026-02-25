using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using System.CodeDom.Compiler;

namespace FlorianAlbert.MySharp.Sdk.Parser.Extensions;

internal static class IndentedTextWriterExtensions
{
    extension(IndentedTextWriter textWriter)
    {
        public void WriteNestedStatement(BoundStatement statement)
        {
            bool needsIndentation = statement.Kind is not BoundNodeKind.BlockStatement;
            if (needsIndentation)
            {
                textWriter.Indent++;
            }

            statement.WriteTo(textWriter);

            if (needsIndentation)
            {
                textWriter.Indent--;
            }
        }

        public void WriteNestedExpression(int parentPrecedence, BoundExpression currentExpression)
        {
            switch (currentExpression.Kind)
            {
                case BoundNodeKind.UnaryExpression:
                    textWriter.WriteNestedExpression(parentPrecedence, SyntaxFacts.GetUnaryOperatorPrecedence(((BoundUnaryExpression) currentExpression).Operator.SyntaxKind), currentExpression);
                    break;
                case BoundNodeKind.BinaryExpression:
                    textWriter.WriteNestedExpression(parentPrecedence, SyntaxFacts.GetBinaryOperatorPrecedence(((BoundBinaryExpression) currentExpression).Operator.SyntaxKind), currentExpression);
                    break;
                default:
                    currentExpression.WriteTo(textWriter);
                    break;
            }
        }

        private void WriteNestedExpression(int parentPrecedence, int currentPrecedence, BoundExpression currentExpression)
        {
            bool needsParenthesis = parentPrecedence >= currentPrecedence;
            if (needsParenthesis)
            {
                textWriter.WritePunctuation(SyntaxKind.OpenParenthesisToken);
            }

            currentExpression.WriteTo(textWriter);

            if (needsParenthesis)
            {
                textWriter.WritePunctuation(SyntaxKind.CloseParenthesisToken);
            }
        }
    }
}
