using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundVariableExpression : BoundExpression
{
    public BoundVariableExpression(VariableSymbol variableSymbol)
    {
        VariableSymbol = variableSymbol;
    }

    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;

    public override TypeSymbol Type => VariableSymbol.Type;

    public VariableSymbol VariableSymbol { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        return [];
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(Type), Type);
        yield return (nameof(VariableSymbol), VariableSymbol);
    }
}
