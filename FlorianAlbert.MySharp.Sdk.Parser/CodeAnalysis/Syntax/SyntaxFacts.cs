namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

public static class SyntaxFacts
{
    public static int GetBinaryOperatorPrecedence(this SyntaxKind kind) => kind switch
    {
        SyntaxKind.StarToken => 7,
        SyntaxKind.SlashToken => 7,
        SyntaxKind.PercentToken => 7,
        SyntaxKind.PlusToken => 6,
        SyntaxKind.MinusToken => 6,
        SyntaxKind.LessLessToken => 5,
        SyntaxKind.GreaterGreaterToken => 5,
        SyntaxKind.EqualsEqualsToken => 4,
        SyntaxKind.BangEqualsToken => 4,
        SyntaxKind.LessToken => 4,
        SyntaxKind.LessOrEqualsToken => 4,
        SyntaxKind.GreaterToken => 4,
        SyntaxKind.GreaterOrEqualsToken => 4,
        SyntaxKind.CaretToken => 3,
        SyntaxKind.AmpersandToken => 2,
        SyntaxKind.AmpersandAmpersandToken => 2,
        SyntaxKind.PipeToken => 1,
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
        SyntaxKind.PlusToken => 8,
        SyntaxKind.MinusToken => 8,
        SyntaxKind.BangToken => 8,
        SyntaxKind.TildeToken => 8,
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
        "if" => SyntaxKind.IfKeyword,
        "else" => SyntaxKind.ElseKeyword,
        "while" => SyntaxKind.WhileKeyword,
        "for" => SyntaxKind.ForKeyword,
        "to" => SyntaxKind.ToKeyword,
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
        SyntaxKind.TildeToken => "~",
        SyntaxKind.AmpersandToken => "&",
        SyntaxKind.PipeToken => "|",
        SyntaxKind.LessLessToken => "<<",
        SyntaxKind.GreaterGreaterToken => ">>",
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
        SyntaxKind.IfKeyword => "if",
        SyntaxKind.ElseKeyword => "else",
        SyntaxKind.WhileKeyword => "while",
        SyntaxKind.ForKeyword => "for",
        SyntaxKind.ToKeyword => "to",
        SyntaxKind.SemicolonToken => ";",
        _ => null
    };
}
