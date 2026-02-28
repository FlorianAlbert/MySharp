namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;

public abstract class ExpressionSyntax : SyntaxNode
{
    protected ExpressionSyntax(SyntaxTree syntaxTree)
        : base(syntaxTree)
    {
    }
}