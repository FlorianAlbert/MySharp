using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Symbols;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Runtime.CompilerServices;
using System.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

internal sealed class Lexer
{
    private readonly SourceText _text;
    private int _position;
    private int _start;
    private SyntaxKind _kind;
    private object? _value;

    public Lexer(SourceText text)
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
        _kind = SyntaxKind.BadCharacterToken;
        _value = null;

        switch (_Current)
        {
            case '\0':
                _kind = SyntaxKind.EndOfFileToken;
                break;
            case '+':
                _kind = SyntaxKind.PlusToken;
                _position++;
                break;
            case '-':
                _kind = SyntaxKind.MinusToken;
                _position++;
                break;
            case '*':
                _kind = SyntaxKind.StarToken;
                _position++;
                break;
            case '/':
                _kind = SyntaxKind.SlashToken;
                _position++;
                break;
            case '%':
                _kind = SyntaxKind.PercentToken;
                _position++;
                break;
            case '(':
                _kind = SyntaxKind.OpenParenthesisToken;
                _position++;
                break;
            case ')':
                _kind = SyntaxKind.CloseParenthesisToken;
                _position++;
                break;
            case '{':
                _kind = SyntaxKind.OpenBraceToken;
                _position++;
                break;
            case '}':
                _kind = SyntaxKind.CloseBraceToken;
                _position++;
                break;
            case '!':
                _position++;
                if (_Current != '=')
                {
                    _kind = SyntaxKind.BangToken;
                }
                else
                {
                    _kind = SyntaxKind.BangEqualsToken;
                    _position++;
                }
                break;
            case '~':
                _kind = SyntaxKind.TildeToken;
                _position++;
                break;
            case '&':
                _position++;
                if (_Current != '&')
                {
                    _kind = SyntaxKind.AmpersandToken;
                }
                else
                {
                    _kind = SyntaxKind.AmpersandAmpersandToken;
                    _position++;
                }
                break;
            case '|':
                _position++;
                if (_Current != '|')
                {
                    _kind = SyntaxKind.PipeToken;
                }
                else
                {
                    _kind = SyntaxKind.PipePipeToken;
                    _position++;
                }
                break;
            case '^':
                _kind = SyntaxKind.CaretToken;
                _position++;
                break;
            case '=':
                _position++;
                if (_Current != '=')
                {
                    _kind = SyntaxKind.EqualsToken;
                }
                else
                {
                    _kind = SyntaxKind.EqualsEqualsToken;
                    _position++;
                }
                break;
            case '<':
                _position++;
                if (_Current == '=')
                {
                    _kind = SyntaxKind.LessOrEqualsToken;
                    _position++;
                }
                else if (_Current == '<')
                {
                    _kind = SyntaxKind.LessLessToken;
                    _position++;
                }
                else
                {
                    _kind = SyntaxKind.LessToken;
                }
                break;
            case '>':
                _position++;
                if (_Current == '=')
                {
                    _kind = SyntaxKind.GreaterOrEqualsToken;
                    _position++;
                }
                else if (_Current == '>')
                {
                    _kind = SyntaxKind.GreaterGreaterToken;
                    _position++;
                }
                else
                {
                    _kind = SyntaxKind.GreaterToken;
                }
                break;
            case ';':
                _kind = SyntaxKind.SemicolonToken;
                _position++;
                break;
            case '"':
                ReadStringToken();
                break;
            case '\'':
                ReadCharacterToken();
                break;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                ReadNumberToken();
                break;
            case ' ':
            case '\t':
            case '\n':
            case '\r':
                ReadWhitespace();
                break;
            case 'a':
            case 'b':
            case 'c':
            case 'd':
            case 'e':
            case 'f':
            case 'g':
            case 'h':
            case 'i':
            case 'j':
            case 'k':
            case 'l':
            case 'm':
            case 'n':
            case 'o':
            case 'p':
            case 'q':
            case 'r':
            case 's':
            case 't':
            case 'u':
            case 'v':
            case 'w':
            case 'x':
            case 'y':
            case 'z':
            case 'A':
            case 'B':
            case 'C':
            case 'D':
            case 'E':
            case 'F':
            case 'G':
            case 'H':
            case 'I':
            case 'J':
            case 'K':
            case 'L':
            case 'M':
            case 'N':
            case 'O':
            case 'P':
            case 'Q':
            case 'R':
            case 'S':
            case 'T':
            case 'U':
            case 'V':
            case 'W':
            case 'X':
            case 'Y':
            case 'Z':
                ReadIdentifierOrKeyword();
                break;
            default:
                if (char.IsWhiteSpace(_Current))
                {
                    // Kept for Unicode support
                    ReadWhitespace();
                }
                else if(char.IsLetter(_Current))
                {
                    // Kept for Unicode support
                    ReadIdentifierOrKeyword();
                }
                else
                {
                    Diagnostics.ReportBadCharacter(_position, _Current);
                    _position++;
                }
                break;
        }

        string? tokenText = SyntaxFacts.GetText(_kind);
        if (tokenText is null)
        {
            int length = _position - _start;
            tokenText = _text.ToString(_start, length);
        }

        return new SyntaxToken(_kind, _start, tokenText, _value);
    }

    private void ReadWhitespace()
    {
        while (char.IsWhiteSpace(_Current))
        {
            _position++;
        }

        _kind = SyntaxKind.WhitespaceToken;
    }

    private void ReadNumberToken()
    {
        while (char.IsDigit(_Current))
        {
            _position++;
        }

        TextSpan span = TextSpan.FromBounds(_start, _position);
        string tokenText = _text.ToString(span);

        if (!int.TryParse(tokenText, out int value))
        {
            Diagnostics.ReportInvalidNumber(span, tokenText, TypeSymbol.Int32);
        }

        _value = value;
        _kind = SyntaxKind.NumberToken;
    }

    private void ReadStringToken()
    {
        _position++;
        StringBuilder stringTokenBuilder = new();
        bool isDone = false;
        while (!isDone)
        {
            switch (_Current)
            {
                case '\0' or '\n' or '\r':
                    Diagnostics.ReportUnterminatedString(TextSpan.FromBounds(_start, _position));
                    isDone = true;
                    break;
                case '\\':
                    TryReadStringEscapeSequence(stringTokenBuilder);
                    _position++;
                    break;
                case '"':
                    isDone = true;
                    _position++;
                    break;
                default:
                    stringTokenBuilder.Append(_Current);
                    _position++;
                    break;
            }
        }

        _value = stringTokenBuilder.ToString();
        _kind = SyntaxKind.StringToken;
    }

    /// <summary>
    /// Tries to read a string escape sequence and appends the corresponding character to the provided StringBuilder.
    /// Otherwise a diagnostic is set.
    /// </summary>
    /// <param name="stringTokenBuilder">The StringBuilder to append the character to.</param>
    private void TryReadStringEscapeSequence(StringBuilder stringTokenBuilder)
    {
        switch (_Lookahead)
        {
            case '"':
            case '\\':
                stringTokenBuilder.Append(_Lookahead);
                _position++;
                break;
            default:
                Diagnostics.ReportInvalidEscapeSequence(new(_position, 1));
                break;
        }
    }

    private void ReadCharacterToken()
    {
        _position++;
        StringBuilder stringTokenBuilder = new();
        bool isDone = false;
        while (!isDone)
        {
            switch (_Current)
            {
                case '\0' or '\n' or '\r':
                    Diagnostics.ReportUnterminatedCharacter(TextSpan.FromBounds(_start, _position));
                    isDone = true;
                    break;
                case '\\':
                    TryReadCharacterEscapeSequence(stringTokenBuilder);
                    _position++;
                    break;
                case '\'':
                    isDone = true;
                    _position++;
                    break;
                default:
                    stringTokenBuilder.Append(_Current);
                    _position++;
                    break;
            }
        }

        if (stringTokenBuilder.Length > 1)
        {
            Diagnostics.ReportTooManyCharactersInCharacterLiteral(TextSpan.FromBounds(_start, _position));
        }

        if (_Current == '\'')
        {
            _value = stringTokenBuilder.ToString()[0];
        }
        else
        {
            _value = '\0';
        }
        _kind = SyntaxKind.CharacterToken;
    }

    /// <summary>
    /// Tries to read a character escape sequence.
    /// If an invalid escape sequence is found, a diagnostic gets set.
    /// </summary>
    /// <param name="value">The out parameter the value gets written to, if successful.</param>
    /// <returns>If the escape sequence got read successfully.</returns>
    private void TryReadCharacterEscapeSequence(StringBuilder stringTokenBuilder)
    {
        switch (_Lookahead)
        {
            case '\'':
            case '\\':
                stringTokenBuilder.Append(_Lookahead);
                _position++;
                break;
            default:
                Diagnostics.ReportInvalidEscapeSequence(new(_position, 1));
                break;
        }
    }

    private void ReadIdentifierOrKeyword()
    {
        while (char.IsLetter(_Current))
        {
            _position++;
        }

        int length = _position - _start;
        string tokenText = _text.ToString(_start, length);

        _kind = SyntaxFacts.GetKeywordKind(tokenText);
    }
}
