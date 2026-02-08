namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundConditionalGotoStatement : BoundStatement
{
    public BoundConditionalGotoStatement(BoundLabel labelSymbol, BoundExpression condition, bool jumpIf = true)
    {
        LabelSymbol = labelSymbol;
        Condition = condition;
        JumpIf = jumpIf;
    }

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;

    public BoundLabel LabelSymbol { get; }

    public BoundExpression Condition { get; }

    public bool JumpIf { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield return Condition;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(LabelSymbol), LabelSymbol);
        yield return (nameof(JumpIf), JumpIf);
    }
}
