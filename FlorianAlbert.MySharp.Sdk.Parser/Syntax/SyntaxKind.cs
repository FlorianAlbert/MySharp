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
    EqualsToken,

    // Keywords
    FalseKeyword,
    TrueKeyword,

    // Expressions
    LiteralExpression,
    NameExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression
}