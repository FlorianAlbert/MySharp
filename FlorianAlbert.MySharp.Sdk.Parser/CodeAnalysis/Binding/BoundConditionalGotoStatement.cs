
namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundConditionalGotoStatement : BoundStatement
{
    public BoundConditionalGotoStatement(LabelSymbol labelSymbol, BoundExpression condition, bool jumpIfFalse = false)
    {
        LabelSymbol = labelSymbol;
        Condition = condition;
        JumpIfFalse = jumpIfFalse;
    }

    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;

    public LabelSymbol LabelSymbol { get; }

    public BoundExpression Condition { get; }

    public bool JumpIfFalse { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield return Condition;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(LabelSymbol), LabelSymbol);
        yield return (nameof(JumpIfFalse), JumpIfFalse);
    }
}
