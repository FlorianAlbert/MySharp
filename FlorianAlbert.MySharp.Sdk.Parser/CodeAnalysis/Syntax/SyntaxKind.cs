namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

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
    OpenBraceToken,
    CloseBraceToken,
    IdentifierToken,
    EqualsToken,
    SemicolonToken,

    // Keywords
    FalseKeyword,
    TrueKeyword,
    LetKeyword,
    VarKeyword,

    // Nodes
    CompilationUnit,

    // Statements
    ExpressionStatement,
    BlockStatement,
    VariableDeclarationStatement,

    // Expressions
    LiteralExpression,
    NameExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression
}