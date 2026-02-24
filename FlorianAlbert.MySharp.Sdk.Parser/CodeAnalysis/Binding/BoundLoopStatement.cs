namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal abstract class BoundLoopStatement : BoundStatement
{
    protected BoundLoopStatement(BoundLabel breakLabel)
    {
        BreakLabel = breakLabel;
    }

    public BoundLabel BreakLabel { get; }
}