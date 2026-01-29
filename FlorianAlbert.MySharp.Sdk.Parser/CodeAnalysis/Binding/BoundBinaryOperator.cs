using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundBinaryOperator
{
    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type operandsType)
        : this(syntaxKind, kind, operandsType, operandsType)
    {
    }

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type operandsType, Type resultType)
        : this(syntaxKind, kind, operandsType, operandsType, resultType)
    {
    }

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, Type leftType, Type rightType, Type resultType)
    {
        SyntaxKind = syntaxKind;
        Kind = kind;
        LeftType = leftType;
        RightType = rightType;
        ResultType = resultType;
    }

    public SyntaxKind SyntaxKind { get; }

    public BoundBinaryOperatorKind Kind { get; }

    public Type LeftType { get; }

    public Type RightType { get; }

    public Type ResultType { get; }

    private static readonly BoundBinaryOperator[] _operators =
    [
        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, typeof(int)),
        new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, typeof(int)),
        new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, typeof(int)),
        new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, typeof(int)),
        new(SyntaxKind.PercentToken, BoundBinaryOperatorKind.Modulo, typeof(int)),
        new(SyntaxKind.CaretToken, BoundBinaryOperatorKind.BitwiseExclusiveOr, typeof(int)),
        new(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, typeof(bool)),
        new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, typeof(bool)),
        new(SyntaxKind.CaretToken, BoundBinaryOperatorKind.BitwiseExclusiveOr, typeof(bool)),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, typeof(int), typeof(bool)),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, typeof(bool), typeof(bool)),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, typeof(int), typeof(bool)),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, typeof(bool), typeof(bool)),
        new(SyntaxKind.LessToken, BoundBinaryOperatorKind.LessThan, typeof(int), typeof(bool)),
        new(SyntaxKind.LessOrEqualsToken, BoundBinaryOperatorKind.LessThanOrEquals, typeof(int), typeof(bool)),
        new(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.GreaterThan, typeof(int), typeof(bool)),
        new(SyntaxKind.GreaterOrEqualsToken, BoundBinaryOperatorKind.GreaterThanOrEquals, typeof(int), typeof(bool))
    ];

    public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, Type? leftType, Type? rightType)
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
