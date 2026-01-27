using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

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
    [InlineData("5 ^ 1;", 4)]
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
    [InlineData("(a = 4) * a / 2;", 8)]
    public void Evaluator_EvaluatesExpression_Correctly(string expression, object expectedResult)
    {
        var syntaxTree = SyntaxTree.Parse(expression);
        Compilation compilation = new(syntaxTree);

        Dictionary<VariableSymbol, object?> variables = [];

        EvaluationResult result = compilation.Evaluate(variables);

        Assert.Empty(result.Diagnostics);
        Assert.Equal(expectedResult, result.Value);
    }
}
