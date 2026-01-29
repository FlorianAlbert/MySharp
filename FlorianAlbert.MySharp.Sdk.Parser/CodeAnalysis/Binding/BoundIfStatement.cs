namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundIfStatement : BoundStatement
{
    public BoundIfStatement(BoundExpression condtion, BoundStatement thenStatement, BoundStatement? elseStatement = null)
    {
        Condition = condtion;
        ThenStatement = thenStatement;
        ElseStatement = elseStatement;
    }

    public override BoundNodeKind Kind => BoundNodeKind.IfStatement;

    public BoundExpression Condition { get; }

    public BoundStatement ThenStatement { get; }

    public BoundStatement? ElseStatement { get; }
}
