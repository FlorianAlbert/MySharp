using FlorianAlbert.MySharp.Sdk.Parser.Extensions;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }

    public abstract IEnumerable<BoundNode> GetChildren();

    public abstract IEnumerable<(string name, object? value)> GetProperties();

    public override string ToString()
    {
        using StringWriter stringWriter = new();

        this.WriteTo(stringWriter);

        return stringWriter.ToString();
    }
}
