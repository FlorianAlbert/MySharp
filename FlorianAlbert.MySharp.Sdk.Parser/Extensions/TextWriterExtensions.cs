using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using System.CodeDom.Compiler;
using System.Numerics;

namespace FlorianAlbert.MySharp.Sdk.Parser.Extensions;

internal static class TextWriterExtensions
{
    extension(TextWriter textWriter)
    {
        public bool IsConsoleOut => textWriter.IsConsoleOutPrivate();

        private bool IsConsoleOutPrivate()
        {
            if (textWriter == Console.Out)
            {
                return true;
            }

            if (textWriter is IndentedTextWriter indentedTextWriter)
            {
                return indentedTextWriter.InnerWriter.IsConsoleOutPrivate();
            }

            return false;
        }

        public void SetForegroundColor(ConsoleColor color)
        {
            if (textWriter.IsConsoleOut)
            {
                Console.ForegroundColor = color;
            }
        }

        public void ResetColor()
        {
            if (textWriter.IsConsoleOut)
            {
                Console.ResetColor();
            }
        }

        public void WriteKeyword(string keyword)
        {
            textWriter.SetForegroundColor(ConsoleColor.Blue);
            textWriter.Write(keyword);
            textWriter.ResetColor();
        }

        public void WriteIdentifier(string identifier)
        {
            textWriter.SetForegroundColor(ConsoleColor.Yellow);
            textWriter.Write(identifier);
            textWriter.ResetColor();
        }

        public void WriteLiteral(object literalValue, TypeSymbol typeSymbol)
        {
            if (typeSymbol == TypeSymbol.BuiltIns.Bool)
            {
                textWriter.WriteKeyword((bool) literalValue ? "true" : "false");
            }
            else if (typeSymbol == TypeSymbol.BuiltIns.Int32)
            {
                textWriter.WriteNumberLiteral((int) literalValue);
            }
            else if (typeSymbol == TypeSymbol.BuiltIns.String)
            {
                textWriter.WriteStringLiteral((string) literalValue);
            }
            else if (typeSymbol == TypeSymbol.BuiltIns.Character)
            {
                textWriter.WriteCharacterLiteral((char) literalValue);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(typeSymbol));
            }
        }

        public void WriteNumberLiteral<TNumber>(INumber<TNumber> numberLiteral)
            where TNumber : INumber<TNumber>
        {
            textWriter.SetForegroundColor(ConsoleColor.Cyan);
            textWriter.Write(numberLiteral);
            textWriter.ResetColor();
        }

        public void WriteStringLiteral(string stringLiteral)
        {
            textWriter.SetForegroundColor(ConsoleColor.Magenta);
            textWriter.Write('"');
            textWriter.Write(stringLiteral);
            textWriter.Write('"');
            textWriter.ResetColor();
        }

        public void WriteCharacterLiteral(char characterLiteral)
        {
            textWriter.SetForegroundColor(ConsoleColor.Magenta);
            textWriter.Write('\'');
            textWriter.Write(characterLiteral);
            textWriter.Write('\'');
            textWriter.ResetColor();
        }

        public void WritePunctuation(string punctuation)
        {
            textWriter.SetForegroundColor(ConsoleColor.DarkGray);
            textWriter.Write(punctuation);
            textWriter.ResetColor();
        }
    }
}
