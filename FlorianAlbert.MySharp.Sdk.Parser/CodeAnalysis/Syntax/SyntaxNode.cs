using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public abstract class SyntaxNode
{
    public abstract SyntaxKind Kind { get; }

    public abstract TextSpan Span { get; }

    public abstract IEnumerable<SyntaxNode> GetChildren();

    public void WriteTo(TextWriter textWriter)
    {
        PrettyPrint(textWriter, this);
    }

    private static void PrettyPrint(TextWriter textWriter, SyntaxNode node, string indent = "", bool isLast = true)
    {
        string indentToWrite = indent;
        if (isLast)
        {
            indentToWrite += "└── ";
            indent += "    ";
        }
        else
        {
            indentToWrite += "├── ";
            indent += "│   ";
        }

        textWriter.Write(indentToWrite);
        textWriter.Write(node.Kind);

        if (node is SyntaxToken token && token.Value is not null)
        {
            textWriter.Write("    ");
            textWriter.Write(token.Value);
        }

        textWriter.WriteLine();

        SyntaxNode? lastChild = node.GetChildren().LastOrDefault();
        foreach (SyntaxNode child in node.GetChildren())
        {
            PrettyPrint(textWriter, child, indent, child == lastChild);
        }
    }

    public override string ToString()
    {
        using StringWriter stringWriter = new();

        WriteTo(stringWriter);

        return stringWriter.ToString();
    }
}