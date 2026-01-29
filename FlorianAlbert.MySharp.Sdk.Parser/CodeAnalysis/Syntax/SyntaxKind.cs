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
    LessToken,
    LessOrEqualsToken,
    GreaterToken,
    GreaterOrEqualsToken,
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
    IfKeyword,
    ElseKeyword,

    // Nodes
    CompilationUnit,
    ElseClause,

    // Statements
    ExpressionStatement,
    BlockStatement,
    VariableDeclarationStatement,
    IfStatement,

    // Expressions
    LiteralExpression,
    NameExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression
}