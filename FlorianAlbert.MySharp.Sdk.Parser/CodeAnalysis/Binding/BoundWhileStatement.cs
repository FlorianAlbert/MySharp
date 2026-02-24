
namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundWhileStatement : BoundLoopStatement
{
    public BoundWhileStatement(BoundExpression condition, BoundStatement statement, BoundLabel breakLabel) : base(breakLabel)
    {
        Condition = condition;
        Body = statement;
    }

    public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;

    public BoundExpression Condition { get; }

    public BoundStatement Body { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield return Condition;
        yield return Body;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield break;
    }
}
