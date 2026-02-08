namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundGotoStatement : BoundStatement
{
    public BoundGotoStatement(BoundLabel labelSymbol)
    {
        LabelSymbol = labelSymbol;
    }

    public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;

    public BoundLabel LabelSymbol { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        return [];
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(LabelSymbol), LabelSymbol);
    }
}
