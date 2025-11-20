namespace FlorianAlbert.MySharp.Syntax;

internal sealed class Lexer
{
    private readonly string _text;
    private int _position;

    private readonly List<string> _diagnostics;

    public Lexer(string text)
    {
        _text = text;
        _position = 0;

        _diagnostics = new List<string>();
    }

    public IEnumerable<string> Diagnostics => _diagnostics;

    private char _Current
    {
        get
        {
            return _position >= _text.Length ? '\0' : _text[_position];
        }
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
                _diagnostics.Add($"The number {tokenText} isn't a valid Int32");
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
            var kind = SyntaxFacts.GetKeywordKind(tokenText);

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
            default:
                char badCharacter = _Current;
                _diagnostics.Add($"Bad character input: {badCharacter}");
                return new SyntaxToken(SyntaxKind.BadCharacterToken, _position++, badCharacter.ToString(), null);
        }
    }
}
