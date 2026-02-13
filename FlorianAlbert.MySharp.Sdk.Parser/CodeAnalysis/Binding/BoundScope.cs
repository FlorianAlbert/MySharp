using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundScope
{
    private Dictionary<string, VariableSymbol>? _variables;
    private Dictionary<string, FunctionSymbol>? _functions;

    public BoundScope? Parent { get; }

    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }

    public bool TryLookupVariable(string name, [NotNullWhen(true)] out VariableSymbol? variable)
    {
        variable = null;
        if (_variables?.TryGetValue(name, out variable) ?? false)
        {
            return true;
        }

        if (Parent is not null)
        {
            return Parent.TryLookupVariable(name, out variable);
        }

        return false;
    }

    public bool TryDeclareVariable(VariableSymbol variable)
    {
        _variables ??= new();

        if (_variables.ContainsKey(variable.Name))
        {
            return false;
        }

        _variables[variable.Name] = variable;
        return true;
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables()
    {
        if (_variables is null)
        {
            return [];
        }

        return [.. _variables.Values];
    }

    public bool TryLookupFunction(string name, [NotNullWhen(true)] out FunctionSymbol? function)
    {
        function = null;
        if (_functions?.TryGetValue(name, out function) ?? false)
        {
            return true;
        }

        if (Parent is not null)
        {
            return Parent.TryLookupFunction(name, out function);
        }

        return false;
    }

    public bool TryDeclareFunction(FunctionSymbol function)
    {
        _functions ??= [];

        if (_functions.ContainsKey(function.Name))
        {
            return false;
        }

        _functions[function.Name] = function;
        return true;
    }

    public ImmutableArray<FunctionSymbol> GetDeclaredFunctions()
    {
        if (_functions is null)
        {
            return [];
        }

        return [.. _functions.Values];
    }
}
