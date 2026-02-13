using System.Collections;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public sealed class SeparatedSyntaxList<T> : IEnumerable<T> where T : SyntaxNode
{
    public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators)
    {
        NodesAndSeparators = nodesAndSeparators;
    }

    public int Count => (NodesAndSeparators.Length + 1) / 2;

    public T this[int index] => (T) NodesAndSeparators[index * 2];

    public ImmutableArray<SyntaxNode> NodesAndSeparators { get; }

    public SyntaxToken GetSeparator(int index) => (SyntaxToken) NodesAndSeparators[index * 2 + 1];

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
