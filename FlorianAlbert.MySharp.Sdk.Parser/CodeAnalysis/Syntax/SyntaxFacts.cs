namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

internal static class SyntaxFacts
{
    public static int GetBinaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.StarToken => 6,
        SyntaxKind.SlashToken => 6,
        SyntaxKind.PercentToken => 6,
        SyntaxKind.PlusToken => 5,
        SyntaxKind.MinusToken => 5,
        SyntaxKind.CaretToken => 4,
        SyntaxKind.EqualsEqualsToken => 3,
        SyntaxKind.BangEqualsToken => 3,
        SyntaxKind.AmpersandAmpersandToken => 2,
        SyntaxKind.PipePipeToken => 1,
        _ => 0
    };

    public static int GetUnaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.PlusToken => 7,
        SyntaxKind.MinusToken => 7,
        SyntaxKind.BangToken => 7,
        _ => 0
    };

    public static SyntaxKind GetKeywordKind(string text) => text switch
    {
        "true" => SyntaxKind.TrueKeyword,
        "false" => SyntaxKind.FalseKeyword,
        _ => SyntaxKind.IdentifierToken
    };
}
