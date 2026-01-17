namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

internal sealed class Lexer
{
    private readonly string _text;
    private int _position;
    private int _start;
    private SyntaxKind _kind;
    private object? _value;

    public Lexer(string text)
    {
        _text = text;
        _position = 0;

        Diagnostics = [];
    }

    public DiagnosticBag Diagnostics { get; }

    private char _Current => Peek(0);

    private char _Lookahead => Peek(1);

    private char Peek(int offset)
    {
        int index = _position + offset;
        return index >= _text.Length ? '\0' : _text[index];
    }

    public SyntaxToken Lex()
    {
        _start = _position;

        if (char.IsDigit(_Current))
        {
            while (char.IsDigit(_Current))
            {
                _position++;
            }

            int length = _position - _start;
            string tokenText = _text.Substring(_start, length);

            if (!int.TryParse(tokenText, out int value))
            {
                Diagnostics.ReportInvalidNumber(new TextSpan(_start, length), tokenText, typeof(int));
            }

            return new SyntaxToken(SyntaxKind.NumberToken, _start, tokenText, value);
        }

        if (char.IsWhiteSpace(_Current))
        {
            while (char.IsWhiteSpace(_Current))
            {
                _position++;
            }

            int length = _position - _start;
            string tokenText = _text.Substring(_start, length);

            return new SyntaxToken(SyntaxKind.WhitespaceToken, _start, tokenText, null);
        }

        if (char.IsLetter(_Current))
        {
            while (char.IsLetter(_Current))
            {
                _position++;
            }

            int length = _position - _start;
            string tokenText = _text.Substring(_start, length);
            SyntaxKind kind = SyntaxFacts.GetKeywordKind(tokenText);

            return new SyntaxToken(kind, _start, tokenText, null);
        }

        switch (_Current)
        {
            case '\0':
                return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);
            case '+':
                return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);
            case '-':
                return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);
            case '*':
                return new SyntaxToken(SyntaxKind.StarToken, _position++, "*", null);
            case '/':
                return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null);
            case '%':
                return new SyntaxToken(SyntaxKind.PercentToken, _position++, "%", null);
            case '(':
                return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(", null);
            case ')':
                return new SyntaxToken(SyntaxKind.CloseParenthesisToken, _position++, ")", null);
            case '!':
                if (_Lookahead == '=')
                {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.BangEqualsToken, _start, "!=", null);
                }
                return new SyntaxToken(SyntaxKind.BangToken, _position++, "!", null);
            case '&':
                if (_Lookahead == '&')
                {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, _start, "&&", null);
                }
                break;
            case '|':
                if (_Lookahead == '|')
                {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.PipePipeToken, _start, "||", null);
                }
                break;
            case '^':
                return new SyntaxToken(SyntaxKind.CaretToken, _position++, "^", null);
            case '=':
                if (_Lookahead == '=')
                {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.EqualsEqualsToken, _start, "==", null);
                }
                return new SyntaxToken(SyntaxKind.EqualsToken, _position++, "=", null);
            default:
                break;
        }

        char badCharacter = _Current;
        Diagnostics.ReportBadCharacter(_position, badCharacter);
        return new SyntaxToken(SyntaxKind.BadCharacterToken, _position++, badCharacter.ToString(), null);
    }
}
