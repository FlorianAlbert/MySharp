using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundBinaryOperator
{
    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol operandsType)
        : this(syntaxKind, kind, operandsType, operandsType)
    {
    }

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol operandsType, TypeSymbol resultType)
        : this(syntaxKind, kind, operandsType, operandsType, resultType)
    {
    }

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol leftType, TypeSymbol rightType, TypeSymbol resultType)
    {
        SyntaxKind = syntaxKind;
        Kind = kind;
        LeftType = leftType;
        RightType = rightType;
        ResultType = resultType;
    }

    public SyntaxKind SyntaxKind { get; }

    public BoundBinaryOperatorKind Kind { get; }

    public TypeSymbol LeftType { get; }

    public TypeSymbol RightType { get; }

    public TypeSymbol ResultType { get; }

    private static readonly BoundBinaryOperator[] _operators =
    [
        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.Int32),
        new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.Int32),
        new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.Int32),
        new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.Int32),
        new(SyntaxKind.PercentToken, BoundBinaryOperatorKind.Modulo, TypeSymbol.Int32),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Int32, TypeSymbol.Bool),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Int32, TypeSymbol.Bool),
        new(SyntaxKind.LessToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Int32, TypeSymbol.Bool),
        new(SyntaxKind.LessOrEqualsToken, BoundBinaryOperatorKind.LessThanOrEquals, TypeSymbol.Int32, TypeSymbol.Bool),
        new(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Int32, TypeSymbol.Bool),
        new(SyntaxKind.GreaterOrEqualsToken, BoundBinaryOperatorKind.GreaterThanOrEquals, TypeSymbol.Int32, TypeSymbol.Bool),
        new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Int32),
        new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Int32),
        new(SyntaxKind.CaretToken, BoundBinaryOperatorKind.BitwiseExclusiveOr, TypeSymbol.Int32),
        new(SyntaxKind.LessLessToken, BoundBinaryOperatorKind.LeftShift, TypeSymbol.Int32),
        new(SyntaxKind.GreaterGreaterToken, BoundBinaryOperatorKind.RightShift, TypeSymbol.Int32),

        new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
        new(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, TypeSymbol.Bool),
        new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
        new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.Bool),
        new(SyntaxKind.CaretToken, BoundBinaryOperatorKind.BitwiseExclusiveOr, TypeSymbol.Bool),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.Bool),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.Bool)
    ];

    public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol leftType, TypeSymbol rightType)
    {
        foreach (BoundBinaryOperator @operator in _operators)
        {
            if (@operator.SyntaxKind == syntaxKind && @operator.LeftType == leftType && @operator.RightType == rightType)
            {
                return @operator;
            }
        }

        return null;
    }
}
