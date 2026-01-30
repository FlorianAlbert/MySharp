using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundAssignmentExpression : BoundExpression
{
    public BoundAssignmentExpression(VariableSymbol variableSymbol, BoundExpression expression)
    {
        VariableSymbol = variableSymbol;
        Expression = expression;
    }

    public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;

    public override Type? Type => Expression.Type;

    public VariableSymbol VariableSymbol { get; }

    public BoundExpression Expression { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        yield return Expression;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        yield return (nameof(Type), Type);
        yield return (nameof(VariableSymbol), VariableSymbol);
    }
}
