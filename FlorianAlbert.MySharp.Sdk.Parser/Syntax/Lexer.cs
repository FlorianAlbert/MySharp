namespace FlorianAlbert.MySharp.Sdk.Parser.Syntax;

internal sealed class Lexer
{
    private readonly string _text;
    private int _position;

    private readonly List<Diagnostic> _diagnostics;

    public Lexer(string text)
    {
        _text = text;
        _position = 0;

        _diagnostics = [];
    }

    public IEnumerable<Diagnostic> Diagnostics => _diagnostics;

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

        if (char.IsDigit(_Current))
        {
            int start = _position;

            while (char.IsDigit(_Current))
            {
                _position++;
            }

            int length = _position - start;
            string tokenText = _text.Substring(start, length);

            if (!int.TryParse(tokenText, out int value))
            {
                Diagnostic intDiagnostic = new($"The number {tokenText} isn't a valid Int32.",
                    start,
                    length);
                _diagnostics.Add(intDiagnostic);
            }

            return new SyntaxToken(SyntaxKind.NumberToken, start, tokenText, value);
        }

        if (char.IsWhiteSpace(_Current))
        {
            int start = _position;

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
            int start = _position;

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
                    return new SyntaxToken(SyntaxKind.BangEqualsToken, _position - 2, "!=", null);
                }
                return new SyntaxToken(SyntaxKind.BangToken, _position++, "!", null);
            case '&':
                if (_Lookahead == '&')
                {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, _position - 2, "&&", null);
                }
                break;
            case '|':
                if (_Lookahead == '|')
                {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.PipePipeToken, _position - 2, "||", null);
                }
                break;
            case '^':
                return new SyntaxToken(SyntaxKind.CaretToken, _position++, "^", null);
            case '=':
                if (_Lookahead == '=')
                {
                    _position += 2;
                    return new SyntaxToken(SyntaxKind.EqualsEqualsToken, _position - 2, "==", null);
                }
                break;
            default:
                break;
        }

        char badCharacter = _Current;
        Diagnostic badCharDiagnostic = new($"Bad character input: {badCharacter}", 
            _position);
        _diagnostics.Add(badCharDiagnostic);
        return new SyntaxToken(SyntaxKind.BadCharacterToken, _position++, badCharacter.ToString(), null);
    }
}
