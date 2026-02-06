using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.Test.CodeAnalysis.Syntax;

internal sealed class AssertingEnumerator : IDisposable
{
    private readonly IEnumerator<SyntaxNode> _enumerator;
    private bool _hasErrors;

    public AssertingEnumerator(SyntaxNode node)
    {
        _enumerator = Flatten(node).GetEnumerator();
    }

    private static IEnumerable<SyntaxNode> Flatten(SyntaxNode node)
    {
        yield return node;
        foreach (SyntaxNode child in node.GetChildren())
        {
            foreach (SyntaxNode descendant in Flatten(child))
            {
                yield return descendant;
            }
        }
    }

    public void Dispose()
    {
        if (!_hasErrors)
        {
            Assert.False(_enumerator.MoveNext(), "Expected no more nodes.");
        }

        _enumerator.Dispose();
    }

    public void AssertToken(SyntaxKind kind, string text)
    {
        try
        {
            Assert.True(_enumerator.MoveNext(), "Expected more nodes.");

            Assert.Equal(kind, _enumerator.Current.Kind);

            SyntaxToken token = Assert.IsType<SyntaxToken>(_enumerator.Current);
            Assert.Equal(text, token.Text);
        }
        catch when (MarkFailed())
        {
            throw;
        }
    }

    public void AssertNode(SyntaxKind kind)
    {
        try
        {
            Assert.True(_enumerator.MoveNext(), "Expected more nodes.");
            Assert.Equal(kind, _enumerator.Current.Kind);
            Assert.IsNotType<SyntaxToken>(_enumerator.Current);
        }
        catch when (MarkFailed())
        {
            throw;
        }
    }

    private bool MarkFailed()
    {
        _hasErrors = true;
        return false;
    }
}
