using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Interpreter.LineRendering;

internal class MySharpLineRenderer : LineRenderer
{
    public override void RenderLine(string line)
    {
        ImmutableArray<SyntaxToken> tokens = SyntaxTree.ParseTokens(line);
        foreach (SyntaxToken token in tokens)
        {
            bool isKeyword = token.Kind.ToString().EndsWith("Keyword", StringComparison.Ordinal);
            bool isNumber = token.Kind == SyntaxKind.NumberToken;
            bool isIdentifier = token.Kind == SyntaxKind.IdentifierToken;

            if (isKeyword)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
            }
            else if (isIdentifier)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (isNumber)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
            }

            Console.Write(token.Text);

            Console.ResetColor();
        }
    }
}
