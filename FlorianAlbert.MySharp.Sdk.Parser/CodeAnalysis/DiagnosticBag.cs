using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Collections;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

internal sealed class DiagnosticBag : IReadOnlyCollection<Diagnostic>
{
    private readonly List<Diagnostic> _diagnostics = [];

    private void Report(TextLocation location, string message)
    {
        Diagnostic diagnostic = new(location, message);
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

    internal void ReportInvalidNumber(TextLocation location, string tokenText)
    {
        string message = $"The number '{tokenText}' is not a valid number.";
        Report(location, message);
    }

    internal void ReportBadCharacter(TextLocation location, char current)
    {
        string message = $"Bad character input: '{current}'.";
        Report(location, message);
    }

    internal void ReportUnexpectedToken(TextLocation location, SyntaxKind actualKind, SyntaxKind expectedKind)
    {
        string message = $"Unexpected token <{actualKind}>, expected <{expectedKind}>.";
        Report(location, message);
    }

    internal void ReportUndefindedBinaryOperator(TextLocation location, string text, TypeSymbol leftType, TypeSymbol rightType)
    {
        string message = $"Binary operator '{text}' is not defined for types '{leftType}' and '{rightType}'.";
        Report(location, message);
    }

    internal void ReportUndefindedUnaryOperator(TextLocation location, string text, TypeSymbol type)
    {
        string message = $"Unary operator '{text}' is not defined for type '{type}'.";
        Report(location, message);
    }

    internal void ReportVariableAlreadyDeclared(TextLocation location, string name)
    {
        string message = $"Variable '{name}' is already declared.";
        Report(location, message);
    }

    internal void ReportUndefinedVariable(TextLocation location, string name)
    {
        string message = $"Undefined variable '{name}'.";
        Report(location, message);
    }

    internal void ReportCannotConvert(TextLocation location, TypeSymbol fromType, TypeSymbol toType)
    {
        string message = $"Cannot convert type '{fromType}' to '{toType}'.";
        Report(location, message);
    }

    internal void ReportCannotAssignToReadOnlyVariable(TextLocation location, string name)
    {
        string message = $"Cannot assign to read-only variable '{name}'.";
        Report(location, message);
    }

    internal void ReportUnterminatedString(TextLocation location)
    {
        string message = "Unterminated string literal.";
        Report(location, message);
    }

    internal void ReportUnterminatedCharacter(TextLocation location)
    {
        string message = "Unterminated character literal.";
        Report(location, message);
    }

    internal void ReportTooManyCharactersInCharacterLiteral(TextLocation location)
    {
        string message = "Too many characters in character literal.";
        Report(location, message);
    }

    internal void ReportInvalidEscapeSequence(TextLocation location)
    {
        string message = $"Invalid escape sequence.";
        Report(location, message);
    }

    internal void ReportUndefinedFunction(TextLocation location, string text)
    {
        string message = $"Undefined function '{text}'.";
        Report(location, message);
    }

    internal void ReportWrongNumberOfArguments(TextLocation location, string name, int length, int count)
    {
        string message = $"Function '{name}' expects {length} argument(s) but was called with {count}.";
        Report(location, message);
    }

    internal void ReportExpressionMustHaveValue(TextLocation location)
    {
        string message = "Expression must have a value.";
        Report(location, message);
    }

    internal void ReportExplicitConversionNeeded(TextLocation location, TypeSymbol fromType, TypeSymbol targetType)
    {
        string message = $"Cannot implicitly convert from {fromType} to {targetType}. An explicit cast is needed.";
        Report(location, message);
    }

    internal void ReportUnexpectedSymbolKind(TextLocation location, string symbolName, SymbolKind expectedSymbolKind, SymbolKind actualSymbolKind)
    {
        string message = $"The symbol '{symbolName}' is not of type '{expectedSymbolKind}' but of type '{actualSymbolKind}'.";
        Report(location, message);
    }

    internal void ReportUndefinedType(TextLocation location, string typeName)
    {
        string message = $"Undefined type '{typeName}'.";
        Report(location, message);
    }

    internal void ReportDuplicateParameterName(TextLocation location, string parameterName)
    {
        string message = $"The parameter name '{parameterName}' is a duplicate.";
        Report(location, message);
    }

    internal void ReportFunctionAlreadyDeclared(TextLocation location, string name)
    {
        string message = $"Function '{name}' is already declared.";
        Report(location, message);
    }

    internal void ReportBreakOrContinueOutsideOfLoop(TextLocation location, string keyword)
    {
        string message = $"The '{keyword}' keyword is only valid in the body of a loop.";
        Report(location, message);
    }

    internal void ReportReturnOutsideOfFunction(TextLocation location)
    {
        string message = "The 'return' keyword is only valid in the body of a function.";
        Report(location, message);
    }

    internal void ReportReturnExpressionNotAllowed(TextLocation location)
    {
        string message = "The 'return' keyword cannot be followed by an expression in a void function.";
        Report(location, message);
    }

    internal void ReportReturnExpressionRequired(TextLocation location, TypeSymbol expectedReturnType)
    {
        string message = $"The 'return' keyword must be followed by an expression of type '{expectedReturnType}'.";
        Report(location, message);
    }

    internal void ReportNotAllPathsReturn(TextLocation location, string name)
    {
        string message = $"Not all code paths in function '{name}' return a value.";
        Report(location, message);
    }
}
