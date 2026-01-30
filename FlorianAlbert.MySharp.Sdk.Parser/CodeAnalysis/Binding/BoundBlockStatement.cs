using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundBlockStatement : BoundStatement
{
    public BoundBlockStatement(ImmutableArray<BoundStatement> statements)
    {
        Statements = statements;
    }

    public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;

    public ImmutableArray<BoundStatement> Statements { get; }

    public override IEnumerable<BoundNode> GetChildren()
    {
        return Statements;
    }

    public override IEnumerable<(string name, object? value)> GetProperties()
    {
        return [];
    }
}
