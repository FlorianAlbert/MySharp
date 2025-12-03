namespace FlorianAlbert.MySharp.Sdk.Parser.Syntax;

public enum SyntaxKind
{
    // Token
    BadCharacterToken,
    EndOfFileToken,
    WhitespaceToken,
    NumberToken,
    PlusToken,
    MinusToken,
    StarToken,
    SlashToken,
    PercentToken,
    BangToken,
    AmpersandAmpersandToken,
    PipePipeToken,
    CaretToken,
    EqualsEqualsToken,
    BangEqualsToken,
    OpenParenthesisToken,
    CloseParenthesisToken,
    IdentifierToken,

    // Keywords
    FalseKeyword,
    TrueKeyword,

    // Expressions
    LiteralExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression
}