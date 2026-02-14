using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal static class ConversionExtensions
{
    extension (Conversion)
    {
        public static Conversion Classify(TypeSymbol from, TypeSymbol to)
        {
            if (from == to)
            {
                return Conversion.Identity;
            }

            if (to == TypeSymbol.String)
            {
                return Conversion.Implicit;
            }

            return Conversion.None;
        }
    }
}
