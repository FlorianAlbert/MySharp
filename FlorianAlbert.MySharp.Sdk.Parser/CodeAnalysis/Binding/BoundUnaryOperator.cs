using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundUnaryOperator
{
    private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol operandType) 
        : this(syntaxKind, kind, operandType, operandType)
    {
    }

    private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol operandType, TypeSymbol resultType)
    {
        SyntaxKind = syntaxKind;
        Kind = kind;
        OperandType = operandType;
        ResultType = resultType;
    }

    public SyntaxKind SyntaxKind { get; }

    public BoundUnaryOperatorKind Kind { get; }

    public TypeSymbol OperandType { get; }

    public TypeSymbol ResultType { get; }

    private static readonly BoundUnaryOperator[] _operators =
    [
        new(SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity, TypeSymbol.Int32),
        new(SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation, TypeSymbol.Int32),
        new(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNegation, TypeSymbol.Bool),
        new(SyntaxKind.TildeToken, BoundUnaryOperatorKind.BitwiseNegation, TypeSymbol.Int32)
    ];

    public static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol operandType)
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
