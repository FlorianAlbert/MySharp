namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

public class VariableSymbol : Symbol
{
    internal VariableSymbol(string name, bool isReadOnly, TypeSymbol type) : base(name)
    {
        IsReadOnly = isReadOnly;
        Type = type;
    }

    public override SymbolKind Kind => SymbolKind.Variable;

    public bool IsReadOnly { get; }

    public TypeSymbol Type { get; }

    public override bool Equals(object? obj)
    {
        if (!base.Equals(obj))
        {
            return false;
        }

        if (obj is not VariableSymbol other)
        {
            return false;
        }

        return IsReadOnly == other.IsReadOnly && Type.Equals(other.Type);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), IsReadOnly, Type);
    }
}
