namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public static class SyntaxFacts
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
        SyntaxKind.LessToken => 3,
        SyntaxKind.LessOrEqualsToken => 3,
        SyntaxKind.GreaterToken => 3,
        SyntaxKind.GreaterOrEqualsToken => 3,
        SyntaxKind.AmpersandAmpersandToken => 2,
        SyntaxKind.PipePipeToken => 1,
        _ => 0
    };

    public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
    {
        foreach (SyntaxKind kind in Enum.GetValues<SyntaxKind>())
        {
            if (GetBinaryOperatorPrecedence(kind) > 0)
            {
                yield return kind;
            }
        }
    }

    public static int GetUnaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.PlusToken => 7,
        SyntaxKind.MinusToken => 7,
        SyntaxKind.BangToken => 7,
        _ => 0
    };

    public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
    {
        foreach (SyntaxKind kind in Enum.GetValues<SyntaxKind>())
        {
            if (GetUnaryOperatorPrecedence(kind) > 0)
            {
                yield return kind;
            }
        }
    }

    public static SyntaxKind GetKeywordKind(string text) => text switch
    {
        "true" => SyntaxKind.TrueKeyword,
        "false" => SyntaxKind.FalseKeyword,
        "let" => SyntaxKind.LetKeyword,
        "var" => SyntaxKind.VarKeyword,
        _ => SyntaxKind.IdentifierToken
    };

    public static string? GetText(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.PlusToken => "+",
        SyntaxKind.MinusToken => "-",
        SyntaxKind.StarToken => "*",
        SyntaxKind.SlashToken => "/",
        SyntaxKind.PercentToken => "%",
        SyntaxKind.BangToken => "!",
        SyntaxKind.AmpersandAmpersandToken => "&&",
        SyntaxKind.PipePipeToken => "||",
        SyntaxKind.CaretToken => "^",
        SyntaxKind.EqualsEqualsToken => "==",
        SyntaxKind.BangEqualsToken => "!=",
        SyntaxKind.LessToken => "<",
        SyntaxKind.LessOrEqualsToken => "<=",
        SyntaxKind.GreaterToken => ">",
        SyntaxKind.GreaterOrEqualsToken => ">=",
        SyntaxKind.OpenParenthesisToken => "(",
        SyntaxKind.CloseParenthesisToken => ")",
        SyntaxKind.OpenBraceToken => "{",
        SyntaxKind.CloseBraceToken => "}",
        SyntaxKind.EqualsToken => "=",
        SyntaxKind.TrueKeyword => "true",
        SyntaxKind.FalseKeyword => "false",
        SyntaxKind.LetKeyword => "let",
        SyntaxKind.VarKeyword => "var",
        SyntaxKind.SemicolonToken => ";",
        _ => null
    };
}
