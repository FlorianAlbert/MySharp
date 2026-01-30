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
    TildeToken,
    AmpersandToken,
    PipeToken,
    BangToken,
    LessLessToken,
    GreaterGreaterToken,
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
    WhileKeyword,
    ForKeyword,
    ToKeyword,

    // Nodes
    CompilationUnit,
    ElseClause,

    // Statements
    BlockStatement,
    VariableDeclarationStatement,
    IfStatement,
    WhileStatement,
    ForStatement,
    ExpressionStatement,

    // Expressions
    LiteralExpression,
    NameExpression,
    AssignmentExpression,
    UnaryExpression,
    BinaryExpression,
    ParenthesizedExpression
}