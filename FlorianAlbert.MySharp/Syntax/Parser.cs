using System.Diagnostics.Metrics;

namespace FlorianAlbert.MySharp.Syntax;

internal sealed class Parser
{
    private readonly SyntaxToken[] _tokens;
    private int _position;

    private readonly List<string> _diagnostics;

    public Parser(string text)
    {
        _position = 0;
        _diagnostics = new List<string>();

        var tokens = new List<SyntaxToken>();

        var lexer = new Lexer(text);

        SyntaxToken token;
        do
        {
            token = lexer.Lex();

            if (token.Kind is not SyntaxKind.WhitespaceToken and not SyntaxKind.BadCharacterToken)
            {
                tokens.Add(token);
            }
        } while (token.Kind is not SyntaxKind.EndOfFileToken);

        _tokens = tokens.ToArray();
        _diagnostics.AddRange(lexer.Diagnostics);
    }

    private SyntaxToken _Current => Peek(0);

    private SyntaxToken Peek(int offset)
    {
        int index = _position + offset;
        return index >= _tokens.Length ? _tokens[^1] : _tokens[index];
    }

    private SyntaxToken NextToken()
    {
        SyntaxToken current = _Current;
        _position++;
        return current;
    }

    private SyntaxToken MatchToken(SyntaxKind kind)
    {
        SyntaxToken token = NextToken();
        if (token.Kind == kind)
        {
            return token;
        }

        _diagnostics.Add($"Unexpected token <{token.Kind}>, expected <{kind}>");
        return new SyntaxToken(kind, _Current.Start, null, null);
    }

    public SyntaxTree Parse()
    {
        ExpressionSyntax expression = ParseExpression();

        SyntaxToken endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);

        return new SyntaxTree(_diagnostics, expression, endOfFileToken);
    }

    private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
    {
        ExpressionSyntax left;

        int unaryOperatorPrecedence = _Current.Kind.GetUnaryOperatorPrecedence();
        if (unaryOperatorPrecedence is not 0 && unaryOperatorPrecedence >= parentPrecedence)
        {
            SyntaxToken operatorToken = NextToken();
            ExpressionSyntax operand = ParseExpression(unaryOperatorPrecedence);

            left = new UnaryExpressionSyntax(operatorToken, operand);
        }
        else
        {
            left = ParseNextPrimaryExpression();
        }

        while (true)
        {
            int precedence = _Current.Kind.GetBinaryOperatorPrecedence();

            if (precedence is 0 || precedence <= parentPrecedence)
            {
                break;
            }

            SyntaxToken operatorToken = NextToken();
            ExpressionSyntax right = ParseExpression(precedence);

            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }

        return left;
    }

    private ExpressionSyntax ParseNextPrimaryExpression()
    {
        switch (_Current.Kind)
        {
            case SyntaxKind.OpenParenthesisToken:
                SyntaxToken openParenthesisToken = NextToken();
                ExpressionSyntax expression = ParseExpression();
                SyntaxToken closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);

                return new ParenthesizedExpressionSyntax(openParenthesisToken, expression, closeParenthesisToken);
            case SyntaxKind.FalseKeyword:
            case SyntaxKind.TrueKeyword:
                SyntaxToken keywordToken = NextToken();
                bool value = keywordToken.Kind == SyntaxKind.TrueKeyword;

                return new LiteralExpressionSyntax(keywordToken, value);
            default:
                SyntaxToken nextNumberToken = MatchToken(SyntaxKind.NumberToken);

                return new LiteralExpressionSyntax(nextNumberToken);
        }
    }
}
