namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

public sealed class TextSpan : IEquatable<TextSpan>
{
    public TextSpan(int start, int length)
    {
        Start = start;
        Length = length;
    }

    public int Start { get; }

    public int Length { get; }

    public int End => Start + Length;

    public static TextSpan FromBounds(int start, int end)
    {
        int length = end - start;
        return new TextSpan(start, length);
    }

    public bool Equals(TextSpan? other)
    {
        if (other is null)
        {
            return false;
        }
        
        return Start == other.Start && Length == other.Length;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TextSpan);
    }

    public override string ToString()
    {
        return $"[{Start}..{End})";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, Length);
    }
}
