using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.Extensions;

internal static class SymbolExtensions
{
    extension(Symbol symbol)
    {
        public void WriteTo(TextWriter textWriter)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Type:
                    WriteTypeTo((TypeSymbol) symbol, textWriter);
                    break;
                case SymbolKind.Variable:
                    WriteVariableTo((VariableSymbol) symbol, textWriter);
                    break;
                case SymbolKind.Function:
                    WriteFunctionTo((FunctionSymbol) symbol, textWriter);
                    break;
                case SymbolKind.Parameter:
                    WriteParameterTo((ParameterSymbol) symbol, textWriter);
                    break;
                default:
                    throw new Exception($"Unexpected symbol '{symbol.Kind}'.");
            }
        }
    }

    private static void WriteTypeTo(TypeSymbol typeSymbol, TextWriter textWriter)
    {
        textWriter.WriteIdentifier(typeSymbol.Name);
    }

    private static void WriteVariableTo(VariableSymbol variableSymbol, TextWriter textWriter)
    {
        textWriter.WriteKeyword(variableSymbol.IsReadOnly ? SyntaxKind.LetKeyword : SyntaxKind.VarKeyword);
        textWriter.WriteSpace();
        textWriter.WriteIdentifier(variableSymbol.Name);
        WriteTypeClause(variableSymbol.Type, textWriter);
    }

    private static void WriteFunctionTo(FunctionSymbol functionSymbol, TextWriter textWriter)
    {
        textWriter.WriteKeyword(SyntaxKind.FunctionKeyword);
        textWriter.WriteSpace();
        textWriter.WriteIdentifier(functionSymbol.Name);
        textWriter.WritePunctuation(SyntaxKind.OpenParenthesisToken);

        for (int parameterIndex = 0; parameterIndex < functionSymbol.Parameters.Length; parameterIndex++)
        {
            if (parameterIndex > 0)
            {
                textWriter.WritePunctuation(SyntaxKind.CommaToken);
                textWriter.WriteSpace();
            }

            ParameterSymbol parameterSymbol = functionSymbol.Parameters[parameterIndex];
            WriteParameterTo(parameterSymbol, textWriter);
        }

        textWriter.WritePunctuation(SyntaxKind.CloseParenthesisToken);

        if (functionSymbol.ReturnType != TypeSymbol.Void)
        {
            WriteTypeClause(functionSymbol.ReturnType, textWriter);
        }
    }

    private static void WriteParameterTo(ParameterSymbol parameterSymbol, TextWriter textWriter)
    {
        textWriter.WriteIdentifier(parameterSymbol.Name);
        WriteTypeClause(parameterSymbol.Type, textWriter);
    }

    private static void WriteTypeClause(TypeSymbol type, TextWriter textWriter)
    {
        textWriter.WriteSpace();
        textWriter.WritePunctuation(SyntaxKind.ColonToken);
        textWriter.WriteSpace();
        WriteTypeTo(type, textWriter);
    }
}
