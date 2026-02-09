namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundLabelStatement : BoundStatement
{
    public BoundLabelStatement(BoundLabel labelSymbol)
    {
        LabelSymbol = labelSymbol;
    }

    public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;

    public BoundLabel LabelSymbol { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield break;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(LabelSymbol), LabelSymbol);
    }
}
