using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal abstract class BoundNode
{
    public abstract BoundNodeKind Kind { get; }

    public abstract IEnumerable<BoundNode> GetChildren();

    public abstract IEnumerable<(string name, object? value)> GetProperties();

    public void WriteTo(TextWriter textWriter)
    {
        PrettyPrint(textWriter, this);
    }

    private static void PrettyPrint(TextWriter textWriter, BoundNode node, string indent = "", bool isLast = true)
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

        WriteNode(textWriter, node);
        WriteProperties(textWriter, node);

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

        BoundNode? lastChild = node.GetChildren().LastOrDefault();
        foreach (BoundNode child in node.GetChildren())
        {
            PrettyPrint(textWriter, child, indent, child == lastChild);
        }
    }

    private static void WriteNode(TextWriter textWriter, BoundNode node)
    {
        bool isConsole = textWriter == Console.Out;
        if (isConsole)
        {
            Console.ForegroundColor = GetColorForNode(node);
        }

        string text = GetText(node);
        textWriter.Write(text);

        if (isConsole)
        {
            Console.ResetColor();
        }
    }

    private static void WriteProperties(TextWriter textWriter, BoundNode node)
    {
        bool isConsole = textWriter == Console.Out;
        bool isFirstProperty = true;
        foreach (var (name, value) in node.GetProperties())
        {
            if (isFirstProperty)
            {
                isFirstProperty = false;
            }
            else
            {
                if (isConsole)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                textWriter.Write(",");
            }

            textWriter.Write(" ");

            if (isConsole)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            textWriter.Write(name);

            if (isConsole)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }
            textWriter.Write(" = ");

            if (isConsole)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
            }
            textWriter.Write(value?.ToString() ?? "null");
        }
    }

    private static string GetText(BoundNode node)
    {
        return node switch
        {
            BoundBinaryExpression boundBinaryExpression => $"{boundBinaryExpression.Operator.Kind}Expression",
            BoundUnaryExpression boundUnaryExpression => $"{boundUnaryExpression.Operator.Kind}Expression",
            _ => node.Kind.ToString(),
        };
    }

    private static ConsoleColor GetColorForNode(BoundNode node)
    {
        return node switch
        {
            BoundExpression => ConsoleColor.Blue,
            BoundStatement => ConsoleColor.Cyan,
            _ => ConsoleColor.Yellow,
        };
    }

    public override string ToString()
    {
        using StringWriter stringWriter = new();

        WriteTo(stringWriter);

        return stringWriter.ToString();
    }
}
