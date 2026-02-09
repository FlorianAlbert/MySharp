using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal abstract class BoundExpression : BoundNode
{
    public abstract TypeSymbol Type { get; }
}
