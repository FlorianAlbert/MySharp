using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public sealed class SyntaxTree
{
    public SyntaxTree(IReadOnlyCollection<Diagnostic> diagnostics, ExpressionSyntax root, SyntaxToken endOfFileToken)
    {
        Diagnostics = diagnostics;
        Root = root;
        EndOfFileToken = endOfFileToken;
    }

    public IReadOnlyCollection<Diagnostic> Diagnostics { get; }
    public ExpressionSyntax Root { get; }
    public SyntaxToken EndOfFileToken { get; }

    public static SyntaxTree Parse(string text)
    {
        var parser = new Parser(text);
        
        return parser.Parse();
    }

    public static IEnumerable<SyntaxToken> ParseTokens(string text)
    {
        var lexer = new Lexer(text);
        while (true)
        {
            SyntaxToken token = lexer.Lex();
            if (token.Kind == SyntaxKind.EndOfFileToken)
            {
                break;
            }

            yield return token;

        }
    }
}
