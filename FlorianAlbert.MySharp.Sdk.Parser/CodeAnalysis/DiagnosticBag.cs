using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Collections;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

internal sealed class DiagnosticBag : IReadOnlyCollection<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = [];

    private void Report(TextSpan span, string message)
    {
        Diagnostic diagnostic = new(span, message);
        _diagnostics.Add(diagnostic);
    }

    public int Count => _diagnostics.Count;

    public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void Add(Diagnostic diagnostic)
    {
        _diagnostics.Add(diagnostic);
    }

    internal void AddRange(DiagnosticBag diagnosticBag)
    {
        _diagnostics.AddRange(diagnosticBag);
    }

    internal void ReportInvalidNumber(TextSpan textSpan, string tokenText, Type type)
    {
        string message = $"The number '{tokenText}' is not a valid {type}.";
        Report(textSpan, message);
    }

    internal void ReportBadCharacter(int position, char current)
    {
        TextSpan span = new(position, 1);
        string message = $"Bad character input: '{current}'.";
        Report(span, message);
    }

    internal void ReportUnexpectedToken(TextSpan span, SyntaxKind actualKind, SyntaxKind expectedKind)
    {
        string message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
        Report(span, message);
    }

    internal void ReportUndefindedBinaryOperator(TextSpan span, string? text, Type? leftType, Type? rightType)
    {
        string message = $"Binary operator '{text}' is not defined for types '{leftType}' and '{rightType}'.";
        Report(span, message);
    }

    internal void ReportUndefindedUnaryOperator(TextSpan span, string? text, Type? type)
    {
        string message = $"Unary operator '{text}' is not defined for type '{type}'.";
        Report(span, message);
    }

    internal void ReportUndefinedName(TextSpan span, string name)
    {
        string message = $"Undefined name '{name}'.";
        Report(span, message);
    }
}
