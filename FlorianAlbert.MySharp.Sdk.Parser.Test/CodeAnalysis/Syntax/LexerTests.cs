using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using FlorianAlbert.MySharp.Sdk.Parser.Test;

namespace FlorianAlbert.MySharp.Sdk.Parser.Test.CodeAnalysis.Syntax;

public class LexerTests
{
    [Theory]
    [MemberData(nameof(GetAllTokens))]
    public void Lexer_LexesTokens_ReturnsCorrectSyntaxToken(SyntaxKind tokenKind, string tokenText)
    {
        IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(tokenText);

        SyntaxToken token = Assert.Single(tokens);
        Assert.Equal(tokenKind, token.Kind);
        Assert.Equal(tokenText, token.Text);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairs))]
    public void Lexer_LexesTokenPairs_ReturnsCorrectSyntaxTokens(SyntaxKind firstTokenKind, string firstTokenText, 
                                                                 SyntaxKind secondTokenKind, string secondTokenText)
    {
        string text = firstTokenText + secondTokenText;
        SyntaxToken[] tokens = [.. SyntaxTree.ParseTokens(text)];

        Assert.Equal(2, tokens.Length);

        Assert.Equal(firstTokenKind, tokens[0].Kind);
        Assert.Equal(firstTokenText, tokens[0].Text);

        Assert.Equal(secondTokenKind, tokens[1].Kind);
        Assert.Equal(secondTokenText, tokens[1].Text);
    }

    [Theory]
    [MemberData(nameof(GetTokenPairsWithSeparator))]
    public void Lexer_LexesTokenPairsWithSeparator_ReturnsCorrectSyntaxTokens(SyntaxKind firstTokenKind, string firstTokenText, 
                                                                              SyntaxKind separatorKind, string separatorText, 
                                                                              SyntaxKind secondTokenKind, string secondTokenText)
    {
        string text = firstTokenText + separatorText + secondTokenText;
        SyntaxToken[] tokens = [.. SyntaxTree.ParseTokens(text)];

        Assert.Equal(3, tokens.Length);

        Assert.Equal(firstTokenKind, tokens[0].Kind);
        Assert.Equal(firstTokenText, tokens[0].Text);

        Assert.Equal(separatorKind, tokens[1].Kind);
        Assert.Equal(separatorText, tokens[1].Text);

        Assert.Equal(secondTokenKind, tokens[2].Kind);
        Assert.Equal(secondTokenText, tokens[2].Text);
    }

    public static TheoryData<SyntaxKind, string> GetTokens()
    {

        TheoryData<SyntaxKind, string> data = new()
        {
            { SyntaxKind.NumberToken, "1" },
            { SyntaxKind.NumberToken, "123" },
            { SyntaxKind.IdentifierToken, "a" },
            { SyntaxKind.IdentifierToken, "abc" },
        };

        IEnumerable<(SyntaxKind, string)> fixedTokens = Enum.GetValues<SyntaxKind>()
            .Select(kind => (kind, text: SyntaxFacts.GetText(kind)))
            .Where(t => t.text is not null)!;

        foreach ((SyntaxKind tokenKind, string tokenText) in fixedTokens)
        {
            data.Add(tokenKind, tokenText);
        }

        return data;
    }

    public static TheoryData<SyntaxKind, string> GetSeparators()
    {
        TheoryData<SyntaxKind, string> data = new()
        {
            { SyntaxKind.WhitespaceToken, " " },
            { SyntaxKind.WhitespaceToken, "  " },
            { SyntaxKind.WhitespaceToken, "\t" },
            { SyntaxKind.WhitespaceToken, "\r" },
            { SyntaxKind.WhitespaceToken, "\n" },
            { SyntaxKind.WhitespaceToken, "\r\n" },
        };
        return data;
    }

    public static TheoryData<SyntaxKind, string> GetAllTokens()
    {
        TheoryData<SyntaxKind, string> data = [];
        foreach ((SyntaxKind tokenKind, string tokenText) in GetTokens().GetTuples())
        {
            data.Add(tokenKind, tokenText);
        }
        foreach ((SyntaxKind separatorKind, string separatorText) in GetSeparators().GetTuples())
        {
            data.Add(separatorKind, separatorText);
        }
        return data;
    }

    public static TheoryData<SyntaxKind, string, SyntaxKind, string> GetTokenPairs()
    {
        TheoryData<SyntaxKind, string, SyntaxKind, string> data = [];
        foreach ((SyntaxKind firstTokenKind, string firstTokenText) in GetAllTokens().GetTuples())
        {
            foreach ((SyntaxKind secondTokenKind, string secondTokenText) in GetAllTokens().GetTuples())
            {
                if (!RequiresSeparator(firstTokenKind, secondTokenKind))
                {
                    data.Add(firstTokenKind, firstTokenText, secondTokenKind, secondTokenText);
                }
            }
        }
        return data;
    }

    public static TheoryData<SyntaxKind, string, SyntaxKind, string, SyntaxKind, string> GetTokenPairsWithSeparator()
    {
        TheoryData<SyntaxKind, string, SyntaxKind, string, SyntaxKind, string> data = [];
        foreach ((SyntaxKind firstTokenKind, string firstTokenText) in GetTokens().GetTuples())
        {
            foreach ((SyntaxKind secondTokenKind, string secondTokenText) in GetTokens().GetTuples())
            {
                if (RequiresSeparator(firstTokenKind, secondTokenKind))
                {
                    foreach ((SyntaxKind separatorKind, string separatorText) in GetSeparators().GetTuples())
                    {
                        data.Add(firstTokenKind, firstTokenText, separatorKind, separatorText, secondTokenKind, secondTokenText);
                    }
                }
            }
        }
        return data;
    }

    private static bool RequiresSeparator(SyntaxKind first, SyntaxKind second)
    {
        bool firstIsKeyword = first.ToString().EndsWith("Keyword");
        bool secondIsKeyword = second.ToString().EndsWith("Keyword");

        if (first is SyntaxKind.IdentifierToken && second is SyntaxKind.IdentifierToken)
        {
            return true;
        }

        if (firstIsKeyword && secondIsKeyword)
        {
            return true;
        }

        if (firstIsKeyword && second is SyntaxKind.IdentifierToken)
        {
            return true;
        }

        if (secondIsKeyword && first is SyntaxKind.IdentifierToken)
        {
            return true;
        }

        if (first is SyntaxKind.WhitespaceToken && second is SyntaxKind.WhitespaceToken)
        {
            return true;
        }

        if (first is SyntaxKind.NumberToken && second is SyntaxKind.NumberToken)
        {
            return true;
        }

        if (first is SyntaxKind.BangToken && second is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
        {
            return true;
        }

        if (first is SyntaxKind.EqualsToken && second is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
        {
            return true;
        }

        return false;
    }
}
