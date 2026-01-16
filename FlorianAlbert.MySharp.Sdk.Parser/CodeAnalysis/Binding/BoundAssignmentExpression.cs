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
}
