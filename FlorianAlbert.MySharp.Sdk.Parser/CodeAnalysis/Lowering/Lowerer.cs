using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Lowering;

internal sealed class Lowerer : BoundTreeRewriter
{
    private Lowerer()
    {
    }

    public static BoundStatement Lower(BoundStatement statement)
    {
        Lowerer lowerer = new();
        return lowerer.RewriteStatement(statement);
    }
}
