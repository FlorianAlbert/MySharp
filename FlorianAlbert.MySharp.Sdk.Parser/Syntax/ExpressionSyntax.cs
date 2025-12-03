namespace FlorianAlbert.MySharp.Sdk.Parser.Syntax;

public abstract class ExpressionSyntax : SyntaxNode
{
    public int Start { get; protected init; }

    public int Length { get; protected init; }
}