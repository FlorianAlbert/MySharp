using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

namespace FlorianAlbert.MySharp.Sdk.Parser.Test.CodeAnalysis.Syntax;

public class SyntaxFactsTests
{
    [Theory]
    [MemberData(nameof(GetAllSyntaxKinds))]
    public void SyntaxFacts_GetText_RoundTrips(SyntaxKind kind)
    {
        string? text = SyntaxFacts.GetText(kind);

        if (text is null)
        {
            return;
        }

        IEnumerable<SyntaxToken> tokens = SyntaxTree.ParseTokens(text);

        SyntaxToken token = Assert.Single(tokens);

        Assert.Equal(kind, token.Kind);
        Assert.Equal(text, token.Text);
    }

    public static TheoryData<SyntaxKind> GetAllSyntaxKinds()
    {
        TheoryData<SyntaxKind> data = [.. Enum.GetValues<SyntaxKind>()];
        return data;
    }
}
