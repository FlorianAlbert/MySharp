namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;

public sealed class TextSpan : IEquatable<TextSpan>
{
    public static readonly IComparer<TextSpan> Comparer = new TextSpanComparer();

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

    private class TextSpanComparer : IComparer<TextSpan>
    {
        public int Compare(TextSpan? first, TextSpan? second)
        {
            if (first is null && second is null)
            {
                return 0;
            }

            if (first is null)
            {
                return -1;
            }

            if (second is null)
            {
                return 1;
            }

            int startComparison = first.Start.CompareTo(second.Start);
            if (startComparison != 0)
            {
                return startComparison;
            }

            return first.Length.CompareTo(second.Length);
        }
    }
}
