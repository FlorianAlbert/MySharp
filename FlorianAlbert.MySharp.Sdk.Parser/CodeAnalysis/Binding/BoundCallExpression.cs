using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundCallExpression : BoundExpression
{
    public BoundCallExpression(FunctionSymbol functionSymbol, ImmutableArray<BoundExpression> arguments)
    {
        FunctionSymbol = functionSymbol;
        Arguments = arguments;
    }

    public override TypeSymbol Type => FunctionSymbol.ReturnType;

    public override BoundNodeKind Kind => BoundNodeKind.CallExpression;

    public FunctionSymbol FunctionSymbol { get; }

    public ImmutableArray<BoundExpression> Arguments { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        foreach (BoundExpression argument in Arguments)
        {
            yield return argument;
        }
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(Type), Type);
        yield return (nameof(FunctionSymbol), FunctionSymbol);
    }
}
