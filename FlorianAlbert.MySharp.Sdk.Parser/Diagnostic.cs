namespace FlorianAlbert.MySharp.Sdk.Parser;

public sealed class Diagnostic
{
    public Diagnostic(string message, int position, int length = 1)
    {
        Message = message;
        Position = position;
        Length = length;
    }

    public string Message { get; }

    public int Position { get; }

    public int Length { get; }

    public override string ToString()
    {
        return $"({Position}, {Length}): {Message}";
    }
}
