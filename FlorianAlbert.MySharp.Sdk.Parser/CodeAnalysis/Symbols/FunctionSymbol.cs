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

    public override bool Equals(object? obj)
    {
        if (!base.Equals(obj))
        {
            return false;
        }

        if (obj is not FunctionSymbol other)
        {
            return false;
        }

        if (!ReturnType.Equals(other.ReturnType))
        {
            return false;
        }

        if (Parameters.Length != other.Parameters.Length)
        {
            return false;
        }

        for (int parameterIndex = 0; parameterIndex < Parameters.Length; parameterIndex++)
        {
            if (!Parameters[parameterIndex].Equals(other.Parameters[parameterIndex]))
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(base.GetHashCode());
        hash.Add(ReturnType);

        foreach (ParameterSymbol parameter in Parameters)
        {
            hash.Add(parameter);
        }

        return hash.ToHashCode();
    }

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
