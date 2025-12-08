namespace FlorianAlbert.MySharp.Sdk.Parser.Syntax;

internal sealed class Lexer
{
    private readonly string _text;
    private int _position;

    private readonly DiagnosticBag _diagnosticBag;

    public Lexer(string text)
    {
        _text = text;
        _position = 0;

        _diagnosticBag = [];
    }

    public DiagnosticBag Diagnostics => _diagnosticBag;

    private char _Current => Peek(0);

    private char _Lookahead => Peek(1);

    private char Peek(int offset)
    {
        int index = _position + offset;
        return index >= _text.Length ? '\0' : _text[index];
    }

    public SyntaxToken Lex()
    {
        if (_position >= _text.Length)
        {
            return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0", null);
        }

        int start = _position;

        if (char.IsDigit(_Current))
        {
            while (char.IsDigit(_Current))
            {
                _position++;
            }

            int length = _position - start;
            string tokenText = _text.Substring(start, length);

            if (!int.TryParse(tokenText, out int value))
            {
                _diagnosticBag.ReportInvalidNumber(new TextSpan(start, length), tokenText, typeof(int));
            }

            return new SyntaxToken(SyntaxKind.NumberToken, start, tokenText, value);
        }

        if (char.IsWhiteSpace(_Current))
        {
            while (char.IsWhiteSpace(_Current))
            {
                _position++;
            }

            int length = _position - start;
            string tokenText = _text.Substring(start, length);

            return new SyntaxToken(SyntaxKind.WhitespaceToken, start, tokenText, null);
        }

        if (char.IsLetter(_Current))
        {
            while (char.IsLetter(_Current))
            {
                _position++;
            }

            int length = _position - start;
            string tokenText = _text.Substring(start, length);
            SyntaxKind kind = SyntaxFacts.GetKeywordKind(tokenText);

            return new SyntaxToken(kind, start, tokenText, null);
        }

        switch (_Current)
        {
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
                    return new SyntaxToken(SyntaxKind.BangEqualsToken, start, "!=", null);
                }
                return new SyntaxToken(SyntaxKind.BangToken, _position++, "!", null);
            case '&':
                if (_Lookahead == '&')
                {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, start, "&&", null);
                }
                break;
            case '|':
                if (_Lookahead == '|')
                {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.PipePipeToken, start, "||", null);
                }
                break;
            case '^':
                return new SyntaxToken(SyntaxKind.CaretToken, _position++, "^", null);
            case '=':
                if (_Lookahead == '=')
                {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.EqualsEqualsToken, start, "==", null);
                }
                break;
            default:
                break;
        }

        char badCharacter = _Current;
        _diagnosticBag.ReportBadCharacter(_position, badCharacter);
        return new SyntaxToken(SyntaxKind.BadCharacterToken, _position++, badCharacter.ToString(), null);
    }
}
