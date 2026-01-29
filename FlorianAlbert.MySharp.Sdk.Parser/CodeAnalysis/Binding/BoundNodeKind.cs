namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
    // Statements
    ExpressionStatement,
    BlockStatement,
    VariableDeclarationStatement,
    IfStatement,
    WhileStatement,

    // Expressions
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression
}