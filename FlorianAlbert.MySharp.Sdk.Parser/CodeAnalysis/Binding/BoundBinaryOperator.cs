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
        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition, TypeSymbol.BuiltIns.Int32),
        new(SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction, TypeSymbol.BuiltIns.Int32),
        new(SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication, TypeSymbol.BuiltIns.Int32),
        new(SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division, TypeSymbol.BuiltIns.Int32),
        new(SyntaxKind.PercentToken, BoundBinaryOperatorKind.Modulo, TypeSymbol.BuiltIns.Int32),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.BuiltIns.Int32, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.BuiltIns.Int32, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.LessToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.BuiltIns.Int32, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.LessOrEqualsToken, BoundBinaryOperatorKind.LessThanOrEquals, TypeSymbol.BuiltIns.Int32, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.BuiltIns.Int32, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.GreaterOrEqualsToken, BoundBinaryOperatorKind.GreaterThanOrEquals, TypeSymbol.BuiltIns.Int32, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.BuiltIns.Int32),
        new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.BuiltIns.Int32),
        new(SyntaxKind.CaretToken, BoundBinaryOperatorKind.BitwiseExclusiveOr, TypeSymbol.BuiltIns.Int32),
        new(SyntaxKind.LessLessToken, BoundBinaryOperatorKind.LeftShift, TypeSymbol.BuiltIns.Int32),
        new(SyntaxKind.GreaterGreaterToken, BoundBinaryOperatorKind.RightShift, TypeSymbol.BuiltIns.Int32),

        new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.CaretToken, BoundBinaryOperatorKind.BitwiseExclusiveOr, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.BuiltIns.Bool),

        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Concatenation, TypeSymbol.BuiltIns.String),
        new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equals, TypeSymbol.BuiltIns.String, TypeSymbol.BuiltIns.Bool),
        new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.NotEquals, TypeSymbol.BuiltIns.String, TypeSymbol.BuiltIns.Bool),

        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Concatenation, TypeSymbol.BuiltIns.Character, TypeSymbol.BuiltIns.String),
        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Concatenation, TypeSymbol.BuiltIns.Character, TypeSymbol.BuiltIns.String, TypeSymbol.BuiltIns.String),
        new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Concatenation, TypeSymbol.BuiltIns.String, TypeSymbol.BuiltIns.Character, TypeSymbol.BuiltIns.String)
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
