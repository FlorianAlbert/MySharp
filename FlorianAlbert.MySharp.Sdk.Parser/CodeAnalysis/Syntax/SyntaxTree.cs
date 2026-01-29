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
