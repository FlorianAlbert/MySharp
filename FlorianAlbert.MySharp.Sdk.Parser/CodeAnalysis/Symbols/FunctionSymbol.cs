using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

public sealed class FunctionSymbol : SymbolWithBuiltIns<FunctionSymbol, FunctionSymbol.BuiltInFunctions>
{
    internal FunctionSymbol(string name, ImmutableArray<ParameterSymbol> parameters, TypeSymbol returnType)
        : base(name)
    {
        Parameters = parameters;
        ReturnType = returnType;
    }

    public override SymbolKind Kind => SymbolKind.Function;

    public ImmutableArray<ParameterSymbol> Parameters { get; }

    public TypeSymbol ReturnType { get; }

    public class BuiltInFunctions : BuiltInSymbols<FunctionSymbol>
    {
        public BuiltInFunctions()
        {
            Print = new("print", [new("value", TypeSymbol.BuiltIns.String)], TypeSymbol.Void);
            Input = new("input", [], TypeSymbol.BuiltIns.String);
            Random = new("random", [new("min", TypeSymbol.BuiltIns.Int32), new("max", TypeSymbol.BuiltIns.Int32)], TypeSymbol.BuiltIns.Int32);

            _all = [Print, Input, Random];
    }

        public readonly FunctionSymbol Print;

        public readonly FunctionSymbol Input;

        public readonly FunctionSymbol Random;

        private readonly ImmutableHashSet<FunctionSymbol> _all;
        public override ImmutableHashSet<FunctionSymbol> GetAll() => _all;
    }
}
