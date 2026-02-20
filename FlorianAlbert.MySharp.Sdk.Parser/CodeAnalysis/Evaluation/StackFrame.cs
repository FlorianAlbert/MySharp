using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Evaluation;

internal sealed class StackFrame
{
    public StackFrame(Dictionary<BoundLabel, int> indexedLabels, Dictionary<VariableSymbol, object?>? initialVariables = null)
    {
        CurrentStatementIndex = 0;
        IndexedLabels = indexedLabels;
        Variables = initialVariables ?? [];
    }

    public int CurrentStatementIndex { get; set; }

    public Dictionary<BoundLabel, int> IndexedLabels { get; }

    public Dictionary<VariableSymbol, object?> Variables { get; }
}
