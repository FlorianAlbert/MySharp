
namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundLiteralExpression : BoundExpression
{
    public BoundLiteralExpression(object? value)
    {
        Value = value;
    }

    public override Type? Type => Value?.GetType();

    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;

    public object? Value { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        return [];
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(Type), Type);
        yield return (nameof(Value), Value);
    }
}
