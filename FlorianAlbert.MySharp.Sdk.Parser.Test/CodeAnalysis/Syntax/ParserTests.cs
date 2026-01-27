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

        ExpressionSyntax expression = ParseExpression(text);

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

    [Theory]
    [MemberData(nameof(GetUnaryOperatorPairs))]
    public void Parser_UnaryExpression_HonorsPrecedences(SyntaxKind unaryOperatorKind, SyntaxKind binaryOperatorKind)
    {
        int unaryOperatorPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(unaryOperatorKind);
        int binaryOperatorPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(binaryOperatorKind);

        string unaryOperatorText = SyntaxFacts.GetText(unaryOperatorKind)!;
        string binaryOperatorText = SyntaxFacts.GetText(binaryOperatorKind)!;

        string text = $"{unaryOperatorText}a {binaryOperatorText} b";

        ExpressionSyntax expression = ParseExpression(text);

        if (unaryOperatorPrecedence >= binaryOperatorPrecedence)
        {
            //    binary
            //    /    \
            // unary    b
            //   |
            //   a

            using AssertingEnumerator enumerator = new(expression);

            enumerator.AssertNode(SyntaxKind.BinaryExpression);
            enumerator.AssertNode(SyntaxKind.UnaryExpression);
            enumerator.AssertToken(unaryOperatorKind, unaryOperatorText);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
            enumerator.AssertToken(binaryOperatorKind, binaryOperatorText);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
        }
        else
        {
            //  unary
            //    |
            //  binary
            //  /   \
            // a     b

            using AssertingEnumerator enumerator = new(expression);

            enumerator.AssertNode(SyntaxKind.UnaryExpression);
            enumerator.AssertToken(unaryOperatorKind, unaryOperatorText);
            enumerator.AssertNode(SyntaxKind.BinaryExpression);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, "a");
            enumerator.AssertToken(binaryOperatorKind, binaryOperatorText);
            enumerator.AssertNode(SyntaxKind.NameExpression);
            enumerator.AssertToken(SyntaxKind.IdentifierToken, "b");
        }
    }

    private static ExpressionSyntax ParseExpression(string text)
    {
        SyntaxTree syntaxTree = SyntaxTree.Parse(text);
        CompilationUnitSyntax root = syntaxTree.Root;
        StatementSyntax statement = root.Statement;
        return Assert.IsType<ExpressionStatementSyntax>(statement).Expression;
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

    public static TheoryData<SyntaxKind, SyntaxKind> GetUnaryOperatorPairs()
    {
        TheoryData<SyntaxKind, SyntaxKind> data = [];
        foreach (SyntaxKind unaryOperatorKind in SyntaxFacts.GetUnaryOperatorKinds())
        {
            foreach (SyntaxKind binaryOperatorKind in SyntaxFacts.GetBinaryOperatorKinds())
            {
                data.Add(unaryOperatorKind, binaryOperatorKind);
            }
        }
        return data;
    }
}
