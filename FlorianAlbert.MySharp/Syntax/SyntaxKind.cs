namespace FlorianAlbert.MySharp.Syntax;

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