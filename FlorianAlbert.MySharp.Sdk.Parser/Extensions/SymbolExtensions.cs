using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

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
        textWriter.WriteKeyword(variableSymbol.IsReadOnly ? "let " : "var ");
        textWriter.WriteIdentifier(variableSymbol.Name);
        textWriter.WritePunctuation(" : ");
        WriteTypeTo(variableSymbol.Type, textWriter);
    }

    private static void WriteFunctionTo(FunctionSymbol functionSymbol, TextWriter textWriter)
    {
        textWriter.WriteKeyword("function ");
        textWriter.WriteIdentifier(functionSymbol.Name);
        textWriter.WritePunctuation("(");

        for (int parameterIndex = 0; parameterIndex < functionSymbol.Parameters.Length; parameterIndex++)
        {
            if (parameterIndex > 0)
            {
                textWriter.WritePunctuation(", ");
            }

            ParameterSymbol parameterSymbol = functionSymbol.Parameters[parameterIndex];
            WriteParameterTo(parameterSymbol, textWriter);
        }

        textWriter.WritePunctuation(")");

        if (functionSymbol.ReturnType != TypeSymbol.Void)
        {
            textWriter.WritePunctuation(" : ");
            WriteTypeTo(functionSymbol.ReturnType, textWriter);
        }
    }

    private static void WriteParameterTo(ParameterSymbol parameterSymbol, TextWriter textWriter)
    {
        textWriter.WriteIdentifier(parameterSymbol.Name);
        textWriter.WritePunctuation(" : ");
        WriteTypeTo(parameterSymbol.Type, textWriter);
    }
}
