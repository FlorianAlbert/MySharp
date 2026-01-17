using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public sealed class SyntaxTree
{
    public SyntaxTree(SourceText sourceText, ImmutableArray<Diagnostic> diagnostics, ExpressionSyntax root, SyntaxToken endOfFileToken)
    {
        SourceText = sourceText;
        Diagnostics = diagnostics;
        Root = root;
        EndOfFileToken = endOfFileToken;
    }

    public SourceText SourceText { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public ExpressionSyntax Root { get; }

    public SyntaxToken EndOfFileToken { get; }

    public static SyntaxTree Parse(string text)
    {
        SourceText sourceText = SourceText.From(text);

        return Parse(sourceText);
    }

    public static SyntaxTree Parse(SourceText text)
    {
        var parser = new Parser(text);

        return parser.Parse();
    }

    public static IEnumerable<SyntaxToken> ParseTokens(string text)
    {
        SourceText sourceText = SourceText.From(text);

        return ParseTokens(sourceText);
    }

    public static IEnumerable<SyntaxToken> ParseTokens(SourceText text)
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
