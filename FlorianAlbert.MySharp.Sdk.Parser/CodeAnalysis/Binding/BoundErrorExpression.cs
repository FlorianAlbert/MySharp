using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal class BoundErrorExpression : BoundExpression
{
    private BoundErrorExpression()
    {
    }

    public static readonly BoundErrorExpression Instance = new();

    public override TypeSymbol Type => TypeSymbol.Error;

    public override BoundNodeKind Kind => BoundNodeKind.ErrorExpression;

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield break;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(Type), Type);
    }
}
