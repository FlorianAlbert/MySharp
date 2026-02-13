using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

public sealed class FunctionSymbol : Symbol
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

    public static class BuiltIns
    {
        public static readonly FunctionSymbol Print = new("print", [new("value", TypeSymbol.String)], TypeSymbol.Void);

        public static readonly FunctionSymbol Input = new("input", [], TypeSymbol.String);

        public static readonly FunctionSymbol Random = new("random", [new("min", TypeSymbol.Int32), new("max", TypeSymbol.Int32)], TypeSymbol.Int32);

        public static ImmutableArray<FunctionSymbol> GetAll() => [Print, Input, Random];
    }
}
