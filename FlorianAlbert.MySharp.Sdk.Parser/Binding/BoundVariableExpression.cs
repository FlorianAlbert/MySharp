namespace FlorianAlbert.MySharp.Sdk.Parser.Binding;

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
