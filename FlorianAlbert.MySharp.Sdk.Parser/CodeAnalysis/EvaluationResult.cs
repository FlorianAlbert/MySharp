namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

public sealed class EvaluationResult
{
    internal EvaluationResult(IReadOnlyCollection<Diagnostic> diagnostics, object? value)
    {
        Diagnostics = diagnostics;
        Value = value;
    }

    public IReadOnlyCollection<Diagnostic> Diagnostics { get; }

    public object? Value { get; }
}