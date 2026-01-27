namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal enum BoundNodeKind
{
    // Statements
    ExpressionStatement,
    BlockStatement,

    // Expressions
    UnaryExpression,
    LiteralExpression,
    BinaryExpression,
    VariableExpression,
    AssignmentExpression
}