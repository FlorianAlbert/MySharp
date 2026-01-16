using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundVariableExpression : BoundExpression
{
    public BoundVariableExpression(VariableSymbol variableSymbol)
    {
        VariableSymbol = variableSymbol;
    }

    public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;

    public override Type Type => VariableSymbol.Type;

    public VariableSymbol VariableSymbol { get; }
}
