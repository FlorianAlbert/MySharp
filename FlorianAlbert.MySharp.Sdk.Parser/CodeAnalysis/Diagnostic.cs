using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

public sealed class Diagnostic
{
    public Diagnostic(TextLocation location, string message)
    {
        Location = location;
        Message = message;
    }

    public TextLocation Location { get; }

    public string Message { get; }

    public override string ToString()
    {
        return Message;
    }
}
