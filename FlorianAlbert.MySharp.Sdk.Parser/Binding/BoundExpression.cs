namespace FlorianAlbert.MySharp.Sdk.Parser.Binding;

internal abstract class BoundExpression : BoundNode
{
    public abstract Type? Type { get; }
}
