using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundScope
{
    private readonly Dictionary<string, VariableSymbol> _variables = [];

    public BoundScope? Parent { get; }

    public BoundScope(BoundScope? parent)
    {
        Parent = parent;
    }

    public bool TryLookup(string name, [NotNullWhen(true)] out VariableSymbol? variable)
    {
        if (_variables.TryGetValue(name, out variable))
        {
            return true;
        }

        if (Parent is not null)
        {
            return Parent.TryLookup(name, out variable);
        }

        return false;
    }

    public bool TryDeclare(VariableSymbol variable)
    {
        if (_variables.ContainsKey(variable.Name))
        {
            return false;
        }

        _variables[variable.Name] = variable;
        return true;
    }

    public ImmutableArray<VariableSymbol> GetDeclaredVariables()
    {
        return [.. _variables.Values];
    }
}
