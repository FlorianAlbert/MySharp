using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.Test.CodeAnalysis;

public class EvaluatorTests
{
    [Theory]
    [InlineData("1;", 1)]
    [InlineData("+1;", 1)]
    [InlineData("-1;", -1)]
    [InlineData("1 + 2;", 3)]
    [InlineData("1 - 2;", -1)]
    [InlineData("2 * 3;", 6)]
    [InlineData("8 / 4;", 2)]
    [InlineData("12 % 5;", 2)]
    [InlineData("1 | 2;", 3)]
    [InlineData("1 | 0;", 1)]
    [InlineData("1 & 2;", 0)]
    [InlineData("1 & 3;", 1)]
    [InlineData("1 & 0;", 0)]
    [InlineData("5 ^ 1;", 4)]
    [InlineData("1 ^ 0;", 1)]
    [InlineData("0 ^ 1;", 1)]
    [InlineData("1 ^ 3;", 2)]
    [InlineData("~1;", -2)]
    [InlineData("8 << 1;", 16)]
    [InlineData("8 << 2;", 32)]
    [InlineData("8 << 3;", 64)]
    [InlineData("8 << 0;", 8)]
    [InlineData("8 >> 1;", 4)]
    [InlineData("8 >> 2;", 2)]
    [InlineData("8 >> 3;", 1)]
    [InlineData("8 >> 0;", 8)]
    [InlineData("(10);", 10)]
    [InlineData("true;", true)]
    [InlineData("false;", false)]
    [InlineData("!true;", false)]
    [InlineData("!false;", true)]
    [InlineData("true && true;", true)]
    [InlineData("true && false;", false)]
    [InlineData("false && true;", false)]
    [InlineData("false && false;", false)]
    [InlineData("true || true;", true)]
    [InlineData("true || false;", true)]
    [InlineData("false || true;", true)]
    [InlineData("false || false;", false)]
    [InlineData("true & true;", true)]
    [InlineData("true & false;", false)]
    [InlineData("false & true;", false)]
    [InlineData("false & false;", false)]
    [InlineData("true | true;", true)]
    [InlineData("true | false;", true)]
    [InlineData("false | true;", true)]
    [InlineData("false | false;", false)]
    [InlineData("true ^ true;", false)]
    [InlineData("true ^ false;", true)]
    [InlineData("false ^ true;", true)]
    [InlineData("false ^ false;", false)]
    [InlineData("5 == 5;", true)]
    [InlineData("421 == 10;", false)]
    [InlineData("39 != 39;", false)]
    [InlineData("27 != 335;", true)]
    [InlineData("true == true;", true)]
    [InlineData("false == false;", true)]
    [InlineData("true == false;", false)]
    [InlineData("false == true;", false)]
    [InlineData("true != true;", false)]
    [InlineData("false != false;", false)]
    [InlineData("true != false;", true)]
    [InlineData("false != true;", true)]
    [InlineData("5 < 10;", true)]
    [InlineData("10 < 5;", false)]
    [InlineData("10 < 10;", false)]
    [InlineData("5 <= 10;", true)]
    [InlineData("10 <= 5;", false)]
    [InlineData("10 <= 10;", true)]
    [InlineData("5 > 10;", false)]
    [InlineData("10 > 5;", true)]
    [InlineData("10 > 10;", false)]
    [InlineData("5 >= 10;", false)]
    [InlineData("10 >= 5;", true)]
    [InlineData("10 >= 10;", true)]
    [InlineData("{ var a = 10; (a = 4) * a / 2; }", 8)]
    [InlineData("{ var a = 0; if (a == 0) a = 10; }", 10)]
    [InlineData("{ var a = 0; if (a == 5) a = 10; }", 0)]
    [InlineData("{ var a = 0; if (a == 0) a = 10; else a = -4; }", 10)]
    [InlineData("{ var a = 0; if (a == 5) a = 10; else a = -4; }", -4)]
    [InlineData("{ var i = 0; var result = 0; while (i < 5) { result = result + i; i = i + 1; } result; }", 10)]
    [InlineData("{ var result = 0; for (let i = 1 to 11) { result = result + i; } result; }", 55)]
    [InlineData("{ var result = 10; for (let i = 1 to (result = result - 1)) { } result; }", 9)]
    public void Evaluator_EvaluatesExpression_Correctly(string expression, object expectedResult)
    {
        AssertValue(expression, expectedResult);
    }

    [Fact]
    public void Evaluator_VariableDeclaration_Reports_Redaclaration()
    {
        string text = @"
            {
                var x = 10;
                let y = 100;
                {
                    var x = 10;
                }
                var [x] = 5;
            }
        ";

        string expectedDiagnosticTexts = @"
            Variable 'x' is already declared.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_Name_Reports_Undefined()
    {
        string text = @"[x] * 10;";

        string expectedDiagnosticTexts = @"
            Undefined name 'x'.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_Assignment_Reports_Undefined()
    {
        string text = @"[x] = 10;";

        string expectedDiagnosticTexts = @"
            Undefined name 'x'.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_Assignment_Reports_Readonly()
    {
        string text = @"
            {
                let y = 100;
                [y = 5];
            }
        ";

        string expectedDiagnosticTexts = @"
            Cannot assign to read-only variable 'y'.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_Assignment_Reports_CannotConvert()
    {
        string text = @"
            {
                var y = 100;
                y [=] true;
            }
        ";

        string expectedDiagnosticTexts = @"
            Cannot convert type 'System.Boolean' to 'System.Int32'.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_UnaryOperator_Reports_Undefined()
    {
        string text = @"[+]true;";

        string expectedDiagnosticTexts = @"
            Unary operator '+' is not defined for type 'System.Boolean'.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_BinaryOperator_Reports_Undefined()
    {
        string text = @"10 [+] true;";

        string expectedDiagnosticTexts = @"
            Binary operator '+' is not defined for types 'System.Int32' and 'System.Boolean'.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_IfStatement_Reports_CannotConvert()
    {
        string text = @"
            {
                if ([10])
                    let x = 10;
            }
        ";

        string expectedDiagnosticTexts = @"
            Cannot convert type 'System.Int32' to 'System.Boolean'.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_WhileStatement_Reports_CannotConvert()
    {
        string text = @"
            {
                var i = 0;
                var result = 0;
                while ([10])
                {
                    i = i + 1;
                    result = result + i;
                }
            }
        ";

        string expectedDiagnosticTexts = @"
            Cannot convert type 'System.Int32' to 'System.Boolean'.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_ForStatement_Reports_CannotConvert_OnLowerBound()
    {
        string text = @"
            {
                var result = 0;
                for (let i = [true] to 5)
                {
                    result = result + i;
                }
            }
        ";

        string expectedDiagnosticTexts = @"
            Cannot convert type 'System.Boolean' to 'System.Int32'.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_ForStatement_Reports_CannotConvert_OnUpperBound()
    {
        string text = @"
            {
                var result = 0;
                for (let i = 0 to [true])
                {
                    result = result + i;
                }
            }
        ";

        string expectedDiagnosticTexts = @"
            Cannot convert type 'System.Boolean' to 'System.Int32'.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    [Fact]
    public void Evaluator_EmptyInput_DoesNotReport_UndefinedName()
    {
        string text = @"[][]";

        string expectedDiagnosticTexts = @"
            Unexpected token <EndOfFileToken>, expected <IdentifierToken>.
            Unexpected token <EndOfFileToken>, expected <SemicolonToken>.
        ";

        AssertDiagnostics(text, expectedDiagnosticTexts);
    }

    private static void AssertValue(string expression, object expectedResult)
    {
        var syntaxTree = SyntaxTree.Parse(expression);
        Compilation compilation = new(syntaxTree);

        Dictionary<VariableSymbol, object?> variables = [];

        EvaluationResult result = compilation.Evaluate(variables);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(expectedResult, result.Value);
    }

    private void AssertDiagnostics(string text, string expectedDiagnosticsText)
    {
        AnnotatedText annotatedText = AnnotatedText.Parse(text);
        SyntaxTree syntaxTree = SyntaxTree.Parse(annotatedText.Text);
        Compilation compilation = new(syntaxTree);
        EvaluationResult result = compilation.Evaluate([]);

        ImmutableArray<string> expectedDiagnostics = AnnotatedText.UnindentLines(expectedDiagnosticsText);

        if (annotatedText.Spans.Length != expectedDiagnostics.Length)
        {
            throw new Exception($"Expected {expectedDiagnostics.Length} diagnostics, but got {annotatedText.Spans.Length}.");
        }

        Assert.Equal(expectedDiagnostics.Length, result.Diagnostics.Length);

        for (int i = 0; i < expectedDiagnostics.Length; i++)
        {
            Diagnostic diagnostic = result.Diagnostics[i];

            string expectedMessage = expectedDiagnostics[i];
            Assert.Equal(expectedMessage, diagnostic.Message);

            TextSpan expectedSpan = annotatedText.Spans[i];
            Assert.Equal(expectedSpan, diagnostic.Span);
        }
    }
}
