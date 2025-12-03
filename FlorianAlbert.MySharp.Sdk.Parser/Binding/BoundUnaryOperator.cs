using FlorianAlbert.MySharp.Sdk.Parser.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.Binding;

internal sealed class BoundUnaryOperator
{
    private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type operandType) 
        : this(syntaxKind, kind, operandType, operandType)
    {
    }

    private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, Type operandType, Type resultType)
    {
        SyntaxKind = syntaxKind;
        Kind = kind;
        OperandType = operandType;
        ResultType = resultType;
    }

    public SyntaxKind SyntaxKind { get; }

    public BoundUnaryOperatorKind Kind { get; }

    public Type OperandType { get; }

    public Type ResultType { get; }

    private static readonly BoundUnaryOperator[] _operators =
    [
        new(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, typeof(int)),
        new(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, typeof(int)),
        new(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, typeof(bool))
    ];

    public static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, Type? operandType)
    {
        foreach (BoundUnaryOperator @operator in _operators)
        {
            if (@operator.SyntaxKind == syntaxKind && @operator.OperandType == operandType)
            {
                return @operator;
            }
        }

        return null;
    }
}
