
namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundForStatement : BoundStatement
{
    public BoundForStatement(VariableSymbol iteratorSymbol,
        BoundExpression lowerBoundExpression,
        BoundExpression upperBoundExpression,
        BoundStatement body)
    {
        IteratorSymbol = iteratorSymbol;
        LowerBound = lowerBoundExpression;
        UpperBound = upperBoundExpression;
        Body = body;
    }

    public override BoundNodeKind Kind => BoundNodeKind.ForStatement;

    public VariableSymbol IteratorSymbol { get; }

    public BoundExpression LowerBound { get; }

    public BoundExpression UpperBound { get; }

    public BoundStatement Body { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield return LowerBound;
        yield return UpperBound;
        yield return Body;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(IteratorSymbol), IteratorSymbol);
    }
}
