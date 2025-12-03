namespace FlorianAlbert.MySharp.Sdk.Parser.Binding;

public abstract class BoundExpression : BoundNode
{
    public abstract Type? Type { get; }
}
