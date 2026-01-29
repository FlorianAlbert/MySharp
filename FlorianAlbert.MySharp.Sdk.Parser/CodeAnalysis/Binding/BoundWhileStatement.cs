namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundWhileStatement : BoundStatement
{
    public BoundWhileStatement(BoundExpression condition, BoundStatement statement)
    {
        Condition = condition;
        Body = statement;
    }

    public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

    public BoundExpression Condition { get; }

    public BoundStatement Body { get; }
}
