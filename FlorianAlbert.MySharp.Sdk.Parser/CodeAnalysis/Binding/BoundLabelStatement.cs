namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundLabelStatement : BoundStatement
{
    public BoundLabelStatement(LabelSymbol labelSymbol)
    {
        LabelSymbol = labelSymbol;
    }

    public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;

    public LabelSymbol LabelSymbol { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        return [];
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(LabelSymbol), LabelSymbol);
    }
}
