using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Text;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

internal sealed class Parser
{
    private readonly SourceText _sourceText;
    private readonly ImmutableArray<SyntaxToken> _tokens;
    private int _position;

    private readonly DiagnosticBag _diagnosticBag;

    public ImmutableArray<Diagnostic> Diagnostics => [.. _diagnosticBag];

    public Parser(SourceText text)
    {
        _sourceText = text;

        _position = 0;
        _diagnosticBag = [];

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

        _tokens = [.. tokens];
        _diagnosticBag.AddRange(lexer.Diagnostics);
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

        _diagnosticBag.ReportUnexpectedToken(token.Span, token.Kind, kind);
        return new SyntaxToken(kind, token.Span.Start, string.Empty, null);
    }

    public CompilationUnitSyntax ParseCompilationUnit()
    {
        StatementSyntax statement = ParseStatement();

        SyntaxToken endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);

        return new CompilationUnitSyntax(statement, endOfFileToken);
    }

    private StatementSyntax ParseStatement()
    {
        if (_Current.Kind == SyntaxKind.OpenBraceToken)
        {
            return ParseBlockStatement();
        }
            
        return ParseExpressionStatement();
    }

    private StatementSyntax ParseExpressionStatement()
    {
        ExpressionSyntax expression = ParseExpression();
        SyntaxToken semicolonToken = MatchToken(SyntaxKind.SemicolonToken);

        return new ExpressionStatementSyntax(expression, semicolonToken);
    }

    private StatementSyntax ParseBlockStatement()
    {
        ImmutableArray<StatementSyntax>.Builder statements = ImmutableArray.CreateBuilder<StatementSyntax>();

        SyntaxToken openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
        while (_Current.Kind is not SyntaxKind.CloseBraceToken and not SyntaxKind.EndOfFileToken)
        {
            StatementSyntax statement = ParseStatement();
            statements.Add(statement);
        }
        SyntaxToken closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);

        return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);
    }

    private ExpressionSyntax ParseExpression() => ParseAssignmentExpression();

    private ExpressionSyntax ParseAssignmentExpression()
    {
        if (_Current.Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.EqualsToken)
        {
            SyntaxToken identifierToken = NextToken();
            SyntaxToken equalsToken = NextToken();
            ExpressionSyntax right = ParseAssignmentExpression();
            return new AssignmentExpressionSyntax(identifierToken, equalsToken, right);
        }

        return ParseBinaryExpression();
    }

    private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
    {
        ExpressionSyntax left;

        int unaryOperatorPrecedence = _Current.Kind.GetUnaryOperatorPrecedence();
        if (unaryOperatorPrecedence is not 0 && unaryOperatorPrecedence >= parentPrecedence)
        {
            SyntaxToken operatorToken = NextToken();
            ExpressionSyntax operand = ParseBinaryExpression(unaryOperatorPrecedence);

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
            ExpressionSyntax right = ParseBinaryExpression(precedence);

            left = new BinaryExpressionSyntax(left, operatorToken, right);
        }

        return left;
    }

    private ExpressionSyntax ParseNextPrimaryExpression()
    {
        return _Current.Kind switch
        {
            SyntaxKind.OpenParenthesisToken => ParseParenthesizedExpression(),
            SyntaxKind.FalseKeyword or SyntaxKind.TrueKeyword => ParseBooleanLiteral(),
            SyntaxKind.NumberToken => ParseNumberLiteral(),
            _ => ParseNameExpression(),
        };
    }

    private LiteralExpressionSyntax ParseNumberLiteral()
    {
        SyntaxToken nextNumberToken = MatchToken(SyntaxKind.NumberToken);

        return new LiteralExpressionSyntax(nextNumberToken);
    }

    private ParenthesizedExpressionSyntax ParseParenthesizedExpression()
    {
        SyntaxToken openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
        ExpressionSyntax expression = ParseExpression();
        SyntaxToken closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);

        return new ParenthesizedExpressionSyntax(openParenthesisToken, expression, closeParenthesisToken);
    }

    private LiteralExpressionSyntax ParseBooleanLiteral()
    {
        bool isTrue = _Current.Kind == SyntaxKind.TrueKeyword;

        SyntaxToken keywordToken = MatchToken(isTrue ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword);

        return new LiteralExpressionSyntax(keywordToken, isTrue);
    }

    private NameExpressionSyntax ParseNameExpression()
    {
        SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(identifierToken);
    }
}
