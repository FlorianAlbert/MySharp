using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

public sealed class TypeSymbol : SymbolWithBuiltIns<TypeSymbol, TypeSymbol.BuiltInTypes>
{
    private TypeSymbol(string name) : base(name)
    {
    }

    public override SymbolKind Kind => SymbolKind.Type;

    public static readonly TypeSymbol Error = new("<error>");

    public static readonly TypeSymbol Void = new("<void>");

    public class BuiltInTypes : BuiltInSymbols<TypeSymbol>
    {
        public readonly TypeSymbol Bool = new("bool");

        public readonly TypeSymbol Int32 = new("int32");

        public readonly TypeSymbol String = new("string");

        public readonly TypeSymbol Character = new("char");

        public override ImmutableArray<TypeSymbol> GetAll() => [Bool, Int32, String, Character];
    }
}
