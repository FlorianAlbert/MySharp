using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundScope
{
    private Dictionary<string, Symbol>? _symbols;

    public BoundScope? Parent { get; }

    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }

    private bool TryLookupSymbol<TSymbol>(string name, [NotNullWhen(true)] out TSymbol? symbol, out bool symbolExists)
        where TSymbol : Symbol
    {
        symbol = null;
        symbolExists = false;
        if (_symbols?.TryGetValue(name, out Symbol? foundSymbol) ?? false)
        {
            symbolExists = true;

            if (foundSymbol is TSymbol typeMatchingSymbol)
            {
                symbol = typeMatchingSymbol;
                return true;
            }

            return false;
        }

        if (Parent is not null)
        {
            return Parent.TryLookupSymbol(name, out symbol, out symbolExists);
        }

        return false;
    }

    private bool TryDeclareSymbol(Symbol symbol)
    {
        _symbols ??= [];

        if (_symbols.ContainsKey(symbol.Name))
        {
            return false;
        }

        _symbols[symbol.Name] = symbol;
        return true;
    }

    public ImmutableArray<TSymbol> GetDeclaredSymbols<TSymbol>()
    {
        if (_symbols is null)
        {
            return [];
        }

        return [.. _symbols.Values.OfType<TSymbol>()];
    }

    public SymbolKind GetSymbolKind(string symbolName)
    {
        if (!TryLookupSymbol(symbolName, out Symbol? symbol, out _))
        {
            throw new Exception($"No symbol with name '{symbolName}' exists.");
        }

        return symbol.Kind;
    }

    public bool TryLookupVariable(string name, [NotNullWhen(true)] out VariableSymbol? variable, out bool symbolExists)
    {
        return TryLookupSymbol(name, out variable, out symbolExists);
    }

    public bool TryDeclareVariable(VariableSymbol variable)
    {
        return TryDeclareSymbol(variable);
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables()
    {
        return GetDeclaredSymbols<VariableSymbol>();
    }

    public bool TryLookupFunction(string name, [NotNullWhen(true)] out FunctionSymbol? function, out bool symbolExists)
    {
        return TryLookupSymbol(name, out function, out symbolExists);
    }

    public bool TryDeclareFunction(FunctionSymbol function)
    {
        return TryDeclareSymbol(function);
    }

    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
    {
        return GetDeclaredSymbols<FunctionSymbol>();
    }
}
