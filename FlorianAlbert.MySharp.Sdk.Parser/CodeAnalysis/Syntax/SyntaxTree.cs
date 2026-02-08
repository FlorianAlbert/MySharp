using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.GeneralNodes;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public sealed class SyntaxTree
{
    private SyntaxTree(SourceText sourceText)
    {
        SourceText = sourceText;

        var parser = new Parser(sourceText);
        Root = parser.ParseCompilationUnit();
        Diagnostics = parser.Diagnostics;
    }

    public SourceText SourceText { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public CompilationUnitSyntax Root { get; }

    public static SyntaxTree Parse(string text)
    {
        SourceText sourceText = SourceText.From(text);

        return Parse(sourceText);
    }

    public static SyntaxTree Parse(SourceText text)
    {
        return new SyntaxTree(text);
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(string text)
    {
        SourceText sourceText = SourceText.From(text);

        return ParseTokens(sourceText);
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(string text, out ImmutableArray<Diagnostic> diagnostics)
    {
        SourceText sourceText = SourceText.From(text);

        return ParseTokens(sourceText, out diagnostics);
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text)
    {
        return ParseTokens(text, out _);
    }

    public static ImmutableArray<SyntaxToken> ParseTokens(SourceText text, out ImmutableArray<Diagnostic> diagnostics)
    {
        static IEnumerable<SyntaxToken> LexTokens(Lexer lexer)
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

        Lexer lexer = new(text);
        ImmutableArray<SyntaxToken> tokens = [.. LexTokens(lexer)];

        diagnostics = [.. lexer.Diagnostics];

        return tokens;
    }
}
