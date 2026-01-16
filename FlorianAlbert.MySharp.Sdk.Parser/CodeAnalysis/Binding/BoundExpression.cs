namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal abstract class BoundExpression : BoundNode
{
    public abstract Type? Type { get; }
}
