using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundVariableDeclarationStatement : BoundStatement
{
    public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression valueExpression)
    {
        Variable = variable;
        ValueExpression = valueExpression;
    }

    public override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;

    public VariableSymbol Variable { get; }

    public BoundExpression ValueExpression { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield return ValueExpression;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(Variable), Variable);
    }
}
