using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal class BoundConversionExpression : BoundExpression
{
    public BoundConversionExpression(BoundExpression boundExpression, TypeSymbol targetType)
    {
        Expression = boundExpression;
        Type = targetType;
    }

    public override TypeSymbol Type { get; }

    public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;

    public BoundExpression Expression { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield return Expression;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(Type), Type);
    }
}
