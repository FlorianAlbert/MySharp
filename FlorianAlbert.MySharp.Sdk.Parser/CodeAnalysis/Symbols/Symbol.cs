using FlorianAlbert.MySharp.Sdk.Parser.Extensions;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

public abstract class Symbol
{
    private protected Symbol(string name)
    {
        Name = name;
    }

    public abstract SymbolKind Kind { get; }

    public string Name { get; }

    public override string ToString()
    {
        using StringWriter stringWriter = new();
        this.WriteTo(stringWriter);
        return stringWriter.ToString();
    }

    public static BuiltInSymbols BuiltIns { get; } = new();

    public class BuiltInSymbols : BuiltInSymbols<Symbol>
    {
        public BuiltInSymbols()
        {
            Functions = FunctionSymbol.BuiltIns;
            Types = TypeSymbol.BuiltIns;

            _all = [.. Functions.GetAll(), .. Types.GetAll()];
        }

        public readonly FunctionSymbol.BuiltInFunctions Functions;

        public readonly TypeSymbol.BuiltInTypes Types;

        private readonly ImmutableHashSet<Symbol> _all;
        public override ImmutableHashSet<Symbol> GetAll() => _all;
    }
}
