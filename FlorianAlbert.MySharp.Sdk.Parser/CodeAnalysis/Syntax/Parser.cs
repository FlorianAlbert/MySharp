using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Expressions;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.GeneralNodes;
using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax.Statements;
using FlorianAlbert.MySharp.Sdk.Parser.Extensions;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;

internal sealed class Parser
{
    private int _position;

    private readonly SyntaxTree _syntaxTree;
    private readonly DiagnosticBag _diagnosticBag;

    public Parser(SyntaxTree syntaxTree)
    {
        _syntaxTree = syntaxTree;
        _position = 0;
        _diagnosticBag = [];

        List<SyntaxToken> tokens = [];

        Lexer lexer = new(syntaxTree);

        SyntaxToken token;
        do
        {
            token = lexer.Lex();

            if (token.Kind is not SyntaxKind.WhitespaceToken and not SyntaxKind.BadCharacterToken)
            {
                tokens.Add(token);
            }
        } while (token.Kind is not SyntaxKind.EndOfFileToken);

        Tokens = [.. tokens];
        _diagnosticBag.AddRange(lexer.Diagnostics);
    }

    public ImmutableArray<SyntaxToken> Tokens { get; }

    public ImmutableArray<Diagnostic> Diagnostics => [.. _diagnosticBag];

    private SyntaxToken _Current => Peek(0);

    private SyntaxToken _Lookahead => Peek(1);

    private SyntaxToken Peek(int offset)
    {
        int index = _position + offset;
        return index >= Tokens.Length ? Tokens[^1] : Tokens[index];
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

        _diagnosticBag.ReportUnexpectedToken(token.Location, token.Kind, kind);
        return new SyntaxToken(_syntaxTree, kind, token.Span.Start, GlobalStringConstants.ConstEmpty, null);
    }

    public CompilationUnitSyntax ParseCompilationUnit()
    {
        ImmutableArray<CompilationUnitSyntaxMember>.Builder compilationUnitMembers = ImmutableArray.CreateBuilder<CompilationUnitSyntaxMember>();
        do
        {
            CompilationUnitSyntaxMember compilationUnitMember = ParseCompilationUnitMember();
            compilationUnitMembers.Add(compilationUnitMember);
        } while (_Current.Kind is not SyntaxKind.EndOfFileToken);

        SyntaxToken endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);

        return new CompilationUnitSyntax(_syntaxTree, compilationUnitMembers.ToImmutable(), endOfFileToken);
    }

    private CompilationUnitSyntaxMember ParseCompilationUnitMember()
    {
        if (_Current.Kind is SyntaxKind.FunctionKeyword)
        {
            return ParseFunctionDefinition();
        }

        return ParseGlobalStatement();
    }

    private FunctionDefinitionSyntax ParseFunctionDefinition()
    {
        SyntaxToken functionKeyword = MatchToken(SyntaxKind.FunctionKeyword);
        SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        SyntaxToken openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
        SeparatedSyntaxList<ParameterSyntax> parameters = ParseSeparatedSyntaxList(ParseParameter);
        SyntaxToken closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);
        TypeClauseSyntax? typeClause = ParseOptionalTypeClause();
        BlockStatementSyntax bodyStatement = ParseBlockStatement();

        return new FunctionDefinitionSyntax(_syntaxTree, functionKeyword,
            identifierToken,
            openParenthesisToken,
            parameters,
            closeParenthesisToken,
            typeClause,
            bodyStatement);
    }

    private ParameterSyntax ParseParameter()
    {
        SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        TypeClauseSyntax typeClause = ParseTypeClause();

        return new ParameterSyntax(_syntaxTree, identifierToken, typeClause);
    }

    private GlobalStatementSyntax ParseGlobalStatement()
    {
        StatementSyntax statement = ParseStatement();
        return new GlobalStatementSyntax(_syntaxTree, statement);
    }

    private StatementSyntax ParseStatement()
    {
        return _Current.Kind switch
        {
            SyntaxKind.OpenBraceToken => ParseBlockStatement(),
            SyntaxKind.LetKeyword or SyntaxKind.VarKeyword => ParseVariableDeclarationStatement(),
            SyntaxKind.IfKeyword => ParseIfStatement(),
            SyntaxKind.WhileKeyword => ParseWhileStatement(),
            SyntaxKind.ForKeyword => ParseForStatement(),
            SyntaxKind.BreakKeyword => ParseBreakStatement(),
            SyntaxKind.ContinueKeyword => ParseContinueStatement(),
            SyntaxKind.ReturnKeyword => ParseReturnStatement(),
            _ => ParseExpressionStatement(),
        };
    }

    private BlockStatementSyntax ParseBlockStatement()
    {
        ImmutableArray<StatementSyntax>.Builder statements = ImmutableArray.CreateBuilder<StatementSyntax>();

        SyntaxToken openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);
        while (_Current.Kind is not SyntaxKind.CloseBraceToken and not SyntaxKind.EndOfFileToken)
        {
            StatementSyntax statement = ParseStatement();
            statements.Add(statement);
        }
        SyntaxToken closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);

        return new BlockStatementSyntax(_syntaxTree, openBraceToken, statements.ToImmutable(), closeBraceToken);
    }

    private VariableDeclarationStatementSyntax ParseVariableDeclarationStatement()
    {
        SyntaxKind expectedKeywordKind = _Current.Kind is SyntaxKind.VarKeyword ? SyntaxKind.VarKeyword : SyntaxKind.LetKeyword;

        SyntaxToken keywordToken = MatchToken(expectedKeywordKind);
        SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        TypeClauseSyntax? typeClause = ParseOptionalTypeClause();
        SyntaxToken equalsToken = MatchToken(SyntaxKind.EqualsToken);
        ExpressionSyntax valueExpression = ParseExpression();
        SyntaxToken semicolonToken = MatchToken(SyntaxKind.SemicolonToken);

        return new VariableDeclarationStatementSyntax(
            _syntaxTree,
            keywordToken,
            identifierToken,
            typeClause,
            equalsToken,
            valueExpression,
            semicolonToken);
    }

    private TypeClauseSyntax? ParseOptionalTypeClause()
    {
        if (_Current.Kind != SyntaxKind.ColonToken)
        {
            return null;
        }

        return ParseTypeClause();
    }

    private TypeClauseSyntax ParseTypeClause()
    {
        SyntaxToken colonToken = MatchToken(SyntaxKind.ColonToken);
        SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        TypeClauseSyntax typeClause = new(_syntaxTree, colonToken, identifierToken);

        return typeClause;
    }

    private IfStatementSyntax ParseIfStatement()
    {
        SyntaxToken ifKeyword = MatchToken(SyntaxKind.IfKeyword);
        SyntaxToken openParenthesesToken = MatchToken(SyntaxKind.OpenParenthesisToken);
        ExpressionSyntax conditionExpression = ParseExpression();
        SyntaxToken closeParenthesesToken = MatchToken(SyntaxKind.CloseParenthesisToken);
        StatementSyntax thenStatement = ParseStatement();
        ElseClauseSyntax? elseClause = ParseOptionalElseClause();

        return new IfStatementSyntax(
            _syntaxTree,
            ifKeyword,
            openParenthesesToken,
            conditionExpression,
            closeParenthesesToken,
            thenStatement,
            elseClause);
    }

    private ElseClauseSyntax? ParseOptionalElseClause()
    {
        if (_Current.Kind != SyntaxKind.ElseKeyword)
        {
            return null;
        }

        SyntaxToken elseKeyword = MatchToken(SyntaxKind.ElseKeyword);
        StatementSyntax elseStatement = ParseStatement();
        ElseClauseSyntax elseClause = new(_syntaxTree, elseKeyword, elseStatement);

        return elseClause;
    }

    private WhileStatementSyntax ParseWhileStatement()
    {
        SyntaxToken whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
        SyntaxToken openParenthesesToken = MatchToken(SyntaxKind.OpenParenthesisToken);
        ExpressionSyntax conditionExpression = ParseExpression();
        SyntaxToken closeParenthesesToken = MatchToken(SyntaxKind.CloseParenthesisToken);
        StatementSyntax bodyStatement = ParseStatement();

        return new WhileStatementSyntax(
            _syntaxTree,
            whileKeyword,
            openParenthesesToken,
            conditionExpression,
            closeParenthesesToken,
            bodyStatement);
    }

    private ForStatementSyntax ParseForStatement()
    {
        SyntaxToken forKeyword = MatchToken(SyntaxKind.ForKeyword);
        SyntaxToken openParenthesesToken = MatchToken(SyntaxKind.OpenParenthesisToken);
        SyntaxToken letKeyword = MatchToken(SyntaxKind.LetKeyword);
        SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        SyntaxToken equalsToken = MatchToken(SyntaxKind.EqualsToken);
        ExpressionSyntax lowerBoundExpression = ParseExpression();
        SyntaxToken toKeyword = MatchToken(SyntaxKind.ToKeyword);
        ExpressionSyntax upperBoundExpression = ParseExpression();
        SyntaxToken closeParenthesesToken = MatchToken(SyntaxKind.CloseParenthesisToken);
        StatementSyntax bodyStatement = ParseStatement();

        return new ForStatementSyntax(
            _syntaxTree,
            forKeyword,
            openParenthesesToken,
            letKeyword,
            identifierToken,
            equalsToken,
            lowerBoundExpression,
            toKeyword,
            upperBoundExpression,
            closeParenthesesToken,
            bodyStatement);
    }

    private BreakStatementSyntax ParseBreakStatement()
    {
        SyntaxToken breakKeyword = MatchToken(SyntaxKind.BreakKeyword);
        SyntaxToken semicolonToken = MatchToken(SyntaxKind.SemicolonToken);

        return new BreakStatementSyntax(_syntaxTree, breakKeyword, semicolonToken);
    }

    private ContinueStatementSyntax ParseContinueStatement()
    {
        SyntaxToken continueKeyword = MatchToken(SyntaxKind.ContinueKeyword);
        SyntaxToken semicolonToken = MatchToken(SyntaxKind.SemicolonToken);

        return new ContinueStatementSyntax(_syntaxTree, continueKeyword, semicolonToken);
    }

    private ReturnStatementSyntax ParseReturnStatement()
    {
        SyntaxToken returnKeyword = MatchToken(SyntaxKind.ReturnKeyword);

        ExpressionSyntax? expression = null;
        if (_Current.Kind is not SyntaxKind.SemicolonToken)
        {
            expression = ParseExpression();
        }

        SyntaxToken semicolonToken = MatchToken(SyntaxKind.SemicolonToken);

        return new ReturnStatementSyntax(_syntaxTree, returnKeyword, expression, semicolonToken);
    }

    private ExpressionStatementSyntax ParseExpressionStatement()
    {
        ExpressionSyntax expression = ParseExpression();
        SyntaxToken semicolonToken = MatchToken(SyntaxKind.SemicolonToken);

        return new ExpressionStatementSyntax(_syntaxTree, expression, semicolonToken);
    }

    private ExpressionSyntax ParseExpression() => ParseAssignmentExpression();

    private ExpressionSyntax ParseAssignmentExpression()
    {
        if (_Current.Kind == SyntaxKind.IdentifierToken && _Lookahead.Kind == SyntaxKind.EqualsToken)
        {
            SyntaxToken identifierToken = NextToken();
            SyntaxToken equalsToken = MatchToken(SyntaxKind.EqualsToken);
            ExpressionSyntax right = ParseAssignmentExpression();
            return new AssignmentExpressionSyntax(_syntaxTree, identifierToken, equalsToken, right);
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

            left = new UnaryExpressionSyntax(_syntaxTree, operatorToken, operand);
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

            left = new BinaryExpressionSyntax(_syntaxTree, left, operatorToken, right);
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
            SyntaxKind.CharacterToken => ParseCharacterLiteral(),
            SyntaxKind.StringToken => ParseStringLiteral(),
            _ => ParseNameOrCallExpression(),
        };
    }

    private LiteralExpressionSyntax ParseNumberLiteral()
    {
        SyntaxToken nextNumberToken = MatchToken(SyntaxKind.NumberToken);

        return new LiteralExpressionSyntax(_syntaxTree, nextNumberToken);
    }

    private LiteralExpressionSyntax ParseCharacterLiteral()
    {
        SyntaxToken nextCharacterToken = MatchToken(SyntaxKind.CharacterToken);

        return new LiteralExpressionSyntax(_syntaxTree, nextCharacterToken);
    }

    private LiteralExpressionSyntax ParseStringLiteral()
    {
        SyntaxToken nextStringToken = MatchToken(SyntaxKind.StringToken);

        return new LiteralExpressionSyntax(_syntaxTree, nextStringToken);
    }

    private ParenthesizedExpressionSyntax ParseParenthesizedExpression()
    {
        SyntaxToken openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);
        ExpressionSyntax expression = ParseExpression();
        SyntaxToken closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);

        return new ParenthesizedExpressionSyntax(_syntaxTree, openParenthesisToken, expression, closeParenthesisToken);
    }

    private LiteralExpressionSyntax ParseBooleanLiteral()
    {
        bool isTrue = _Current.Kind == SyntaxKind.TrueKeyword;

        SyntaxToken keywordToken = MatchToken(isTrue ? SyntaxKind.TrueKeyword : SyntaxKind.FalseKeyword);

        return new LiteralExpressionSyntax(_syntaxTree, keywordToken, isTrue);
    }

    private ExpressionSyntax ParseNameOrCallExpression()
    {
        if (_Lookahead.Kind == SyntaxKind.OpenParenthesisToken)
        {
            return ParseCallExpression();
        }
        else
        {
            return ParseNameExpression();
        }
    }

    private CallExpressionSyntax ParseCallExpression()
    {
        SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        SyntaxToken openParenthesisToken = MatchToken(SyntaxKind.OpenParenthesisToken);

        SeparatedSyntaxList<ExpressionSyntax> arguments = ParseSeparatedSyntaxList(ParseExpression);

        SyntaxToken closeParenthesisToken = MatchToken(SyntaxKind.CloseParenthesisToken);

        return new CallExpressionSyntax(_syntaxTree, identifierToken, openParenthesisToken, arguments, closeParenthesisToken);
    }

    private SeparatedSyntaxList<TNode> ParseSeparatedSyntaxList<TNode>(Func<TNode> nodeParser)
        where TNode : SyntaxNode
    {
        ImmutableArray<SyntaxNode>.Builder nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();

        while (_Current.Kind is not SyntaxKind.CloseParenthesisToken and not SyntaxKind.EndOfFileToken)
        {
            TNode node = nodeParser();
            nodesAndSeparators.Add(node);

            if (_Current.Kind is not SyntaxKind.CloseParenthesisToken)
            {
                SyntaxToken commaToken = MatchToken(SyntaxKind.CommaToken);
                nodesAndSeparators.Add(commaToken);
            }
        }

        return new SeparatedSyntaxList<TNode>(nodesAndSeparators.ToImmutable());
    }

    private NameExpressionSyntax ParseNameExpression()
    {
        SyntaxToken identifierToken = MatchToken(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(_syntaxTree, identifierToken);
    }
}
