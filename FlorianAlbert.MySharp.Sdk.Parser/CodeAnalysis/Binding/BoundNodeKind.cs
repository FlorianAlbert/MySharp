namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
    // Statements
    ExpressionStatement,
    BlockStatement,
    VariableDeclarationStatement,
    IfStatement,

    // Expressions
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression
}