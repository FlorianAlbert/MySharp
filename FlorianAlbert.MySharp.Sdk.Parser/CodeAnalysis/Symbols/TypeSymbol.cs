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
        public BuiltInTypes()
        {
            Bool = new("bool");
            Int32 = new("int32");
            String = new("string");
            Character = new("char");

            _all = [Bool, Int32, String, Character];
        }

        public readonly TypeSymbol Bool;

        public readonly TypeSymbol Int32;

        public readonly TypeSymbol String;

        public readonly TypeSymbol Character;

        private readonly ImmutableHashSet<TypeSymbol> _all;
        public override ImmutableHashSet<TypeSymbol> GetAll() => _all;
    }
}
