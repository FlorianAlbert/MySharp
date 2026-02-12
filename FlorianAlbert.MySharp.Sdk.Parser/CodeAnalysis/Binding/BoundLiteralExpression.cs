
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundLiteralExpression : BoundExpression
{
    public BoundLiteralExpression(object value)
    {
        Value = value;

        Type = value switch
        {
            bool => TypeSymbol.Bool,
            int => TypeSymbol.Int32,
            string => TypeSymbol.String,
            char => TypeSymbol.Character,
            _ => throw new InvalidOperationException($"Unexpected literal type: {value.GetType()}")
        };
    }

    public override TypeSymbol Type { get; }

    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;

    public object Value { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield break;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(Type), Type);
        yield return (nameof(Value), Value);
    }
}
