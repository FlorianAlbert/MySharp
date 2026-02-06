using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using FlorianAlbert.MySharp.Sdk.Parser.Extensions;

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

    private static void PrettyPrint(TextWriter textWriter, SyntaxNode node, string indent = GlobalStringConstants.ConstEmpty, bool isLast = true)
    {
        bool isConsole = textWriter == Console.Out;

        string marker;
        if (isLast)
        {
            marker = "└── ";
        }
        else
        {
            marker = "├── ";
        }

        if (isConsole)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        textWriter.Write(indent);
        
        textWriter.Write(marker);

        if (isConsole)
        {
            Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Cyan : ConsoleColor.Yellow;
        }

        textWriter.Write(node.Kind);

        if (node is SyntaxToken token && token.Value is not null)
        {
            textWriter.Write("    ");
            textWriter.Write(token.Value);
        }

        if (isConsole)
        {
            Console.ResetColor();
        }

        textWriter.WriteLine();

        if (isLast)
        {
            indent += "    ";
        }
        else
        {
            indent += "│   ";
        }

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