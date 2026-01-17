using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.Test.CodeAnalysis.Syntax;

public class ParserTests
{
    [Theory]
    [MemberData(nameof(GetBinaryOperatorPairs))]
    public void Parser_BinaryExpression_HonorsPrecedences(SyntaxKind firstOperatorKind, SyntaxKind secondOperatorKind)
    {
        int firstOperatorPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(firstOperatorKind);
        int secondOperatorPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(secondOperatorKind);

        string firstOperatorText = SyntaxFacts.GetText(firstOperatorKind)!;
        string secondOperatorText = SyntaxFacts.GetText(secondOperatorKind)!;

        string text = $"a {firstOperatorText} b {secondOperatorText} c";

        ExpressionSyntax expression = SyntaxTree.Parse(text).Root;

        if (firstOperatorPrecedence >= secondOperatorPrecedence)
        {
            //     op2
            //    /   \
            //   op1   c
            //  /   \
            // a     b

            using AssertingEnumerator enumerator = new(expression);

            enumerator.AssertNode(SyntaxKind.BinaryExpression);
            enumerator.AssertNode(SyntaxKind.BinaryExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
            enumerator.AssertToken(firstOperatorKind, firstOperatorText);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
            enumerator.AssertToken(secondOperatorKind, secondOperatorText);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, "c");
        }
        else
        {
            //     op1
            //    /   \
            //   a    op2
            //        /  \
            //       b    c

            using AssertingEnumerator enumerator = new(expression);

            enumerator.AssertNode(SyntaxKind.BinaryExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
            enumerator.AssertToken(firstOperatorKind, firstOperatorText);
            enumerator.AssertNode(SyntaxKind.BinaryExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
            enumerator.AssertToken(secondOperatorKind, secondOperatorText);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, "c");
        }
    }

    public static TheoryData<SyntaxKind, SyntaxKind> GetBinaryOperatorPairs()
    {
        IEnumerable<SyntaxKind> binaryOperatorKinds = SyntaxFacts.GetBinaryOperatorKinds();
        TheoryData<SyntaxKind, SyntaxKind> data = [];
        foreach (SyntaxKind firstOperatorKind in binaryOperatorKinds)
        {
            foreach (SyntaxKind secondOperatorKind in binaryOperatorKinds)
            {
                data.Add(firstOperatorKind, secondOperatorKind);
            }
        }
        return data;
    }
}
