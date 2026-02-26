using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class ControlFlowGraph
{
    public ControlFlowGraph(BasicBlock startBlock, BasicBlock endBlock, ImmutableArray<BasicBlock> blocks, ImmutableArray<BasicBlockEdge> edges)
    {
        StartBlock = startBlock;
        EndBlock = endBlock;
        Blocks = blocks;
        Edges = edges;
    }

    public BasicBlock StartBlock { get; }

    public BasicBlock EndBlock { get; }

    public ImmutableArray<BasicBlock> Blocks { get; }

    public ImmutableArray<BasicBlockEdge> Edges { get; }

    public bool AllPathsReturn
    {
        get
        {
            if (Edges.Length == 1 && Edges[0].FromBlock == StartBlock && Edges[0].ToBlock == EndBlock)
            {
                return false;
            }

            foreach (BasicBlockEdge edge in EndBlock.IncomingEdges)
            {
                if (edge.FromBlock.Statements.Length is 0 || edge.FromBlock.Statements[^1].Kind is not BoundNodeKind.ReturnStatement)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public static ControlFlowGraph Create(BoundBlockStatement body)
    {
        Builder builder = new();
        return builder.Build(body);
    }

    public sealed class BasicBlock
    {
        private readonly Lazy<ImmutableArray<BasicBlockEdge>> _incomingEdges;
        private readonly Lazy<ImmutableArray<BasicBlockEdge>> _outgoingEdges;

        internal BasicBlock(
            ImmutableArray<BoundStatement> statements,
            Func<ImmutableArray<BasicBlockEdge>> incomingEdgesProvider,
            Func<ImmutableArray<BasicBlockEdge>> outgoingEdgesProvider)
        {
            Statements = statements;
            _incomingEdges = new(incomingEdgesProvider);
            _outgoingEdges = new(outgoingEdgesProvider);
        }

        public ImmutableArray<BasicBlockEdge> IncomingEdges => _incomingEdges.Value;

        public ImmutableArray<BasicBlockEdge> OutgoingEdges => _outgoingEdges.Value;

        public ImmutableArray<BoundStatement> Statements { get; }
    }

    public sealed class BasicBlockEdge
    {
        public BasicBlockEdge(BasicBlock fromBlock, BasicBlock toBlock, BoundExpression? condition)
        {
            FromBlock = fromBlock;
            ToBlock = toBlock;
            Condition = condition;
        }

        public BasicBlock FromBlock { get; }

        public BasicBlock ToBlock { get; }

        public BoundExpression? Condition { get; }
    }

    private sealed class Builder
    {
        private readonly Dictionary<BasicBlock, ImmutableArray<BasicBlockEdge>.Builder> _blockIncomingEdges = [];
        private readonly Dictionary<BasicBlock, ImmutableArray<BasicBlockEdge>.Builder> _blockOutgoingEdges = [];

        public ControlFlowGraph Build(BoundBlockStatement body)
        {
            ImmutableArray<BasicBlock> blocks = ComputeBlocks(body);

            ImmutableDictionary<BoundLabel, BasicBlock> labelToBlockMap = IndexLabelsForBlocks(blocks);

            BasicBlock startBlock = CreateBlock([]);
            BasicBlock endBlock = CreateBlock([]);

            ImmutableArray<BasicBlockEdge> edges = ComputeEdges(startBlock, endBlock, blocks, labelToBlockMap);

            ImmutableArray<BasicBlock> trimmedBlocks = TrimBlocks(blocks);

            ImmutableArray<BasicBlock> allBlocks = [startBlock, .. trimmedBlocks, endBlock];

            ImmutableArray<BasicBlockEdge> trimmedEdges = TrimEdges(edges, allBlocks);

            return new ControlFlowGraph(startBlock, endBlock, allBlocks, trimmedEdges);
        }

        private ImmutableArray<BasicBlockEdge> TrimEdges(ImmutableArray<BasicBlockEdge> edges, ImmutableArray<BasicBlock> trimmedBlocks)
        {
            ImmutableArray<BasicBlockEdge>.Builder trimmedEdges = edges.ToBuilder();

            foreach (BasicBlockEdge edge in edges)
            {
                if (!trimmedBlocks.Contains(edge.FromBlock) || !trimmedBlocks.Contains(edge.ToBlock))
                {
                    trimmedEdges.Remove(edge);
                }
            }

            return trimmedEdges.ToImmutable();
        }

        private ImmutableArray<BasicBlock> TrimBlocks(ImmutableArray<BasicBlock> blocks)
        {
            ImmutableArray<BasicBlock>.Builder trimmedBlocks = blocks.ToBuilder();

            bool blockGotRemoved;
            do
            {
                blockGotRemoved = RemoveNextUnreachableBlock(trimmedBlocks);
            } while (blockGotRemoved);

            return trimmedBlocks.ToImmutable();
        }

        private bool RemoveNextUnreachableBlock(ImmutableArray<BasicBlock>.Builder trimmedBlocks)
        {
            foreach (BasicBlock block in trimmedBlocks)
            {
                ImmutableArray<BasicBlockEdge>.Builder incomingEdges = _blockIncomingEdges[block];
                if (incomingEdges.Count == 0)
                {
                    RemoveBlock(block);
                    trimmedBlocks.Remove(block);
                    return true;
                }
            }

            return false;
        }

        private void RemoveBlock(BasicBlock block)
        {
            foreach (BasicBlockEdge incomingEdge in _blockIncomingEdges[block])
            {
                BasicBlock fromBlock = incomingEdge.FromBlock;
                _blockOutgoingEdges[fromBlock].Remove(incomingEdge);
            }

            _blockIncomingEdges.Remove(block);

            foreach (BasicBlockEdge outgoingEdge in _blockOutgoingEdges[block])
            {
                BasicBlock toBlock = outgoingEdge.ToBlock;
                _blockIncomingEdges[toBlock].Remove(outgoingEdge);
            }

            _blockOutgoingEdges.Remove(block);
        }

        private ImmutableArray<BasicBlockEdge> ComputeEdges(BasicBlock startBlock, BasicBlock endBlock, ImmutableArray<BasicBlock> blocks, ImmutableDictionary<BoundLabel, BasicBlock> labelToBlockMap)
        {
            ImmutableArray<BasicBlockEdge>.Builder edges = ImmutableArray.CreateBuilder<BasicBlockEdge>();
            if (blocks.Length == 0)
            {
                BasicBlockEdge edge = Connect(startBlock, endBlock);
                edges.Add(edge);
                return edges.ToImmutable();
            }
            else
            {
                BasicBlockEdge edge = Connect(startBlock, blocks[0]);
                edges.Add(edge);
            }

            for (int blockIndex = 0; blockIndex < blocks.Length; blockIndex++)
            {
                BasicBlock currentBlock = blocks[blockIndex];
                BasicBlock nextBlock = blockIndex < blocks.Length - 1 ? blocks[blockIndex + 1] : endBlock;

                AddOutgoingEdgesForBlock(currentBlock, nextBlock, endBlock, labelToBlockMap, edges);
            }

            return edges.ToImmutable();
        }

        private void AddOutgoingEdgesForBlock(BasicBlock currentBlock, BasicBlock nextBlock, BasicBlock endBlock, ImmutableDictionary<BoundLabel, BasicBlock> labelToBlockMap, ImmutableArray<BasicBlockEdge>.Builder edges)
        {
            if (currentBlock.Statements.Length == 0)
            {
                BasicBlockEdge edge = Connect(currentBlock, nextBlock);
                edges.Add(edge);
                return;
            }

            BoundStatement lastStatement = currentBlock.Statements[^1];
            switch (lastStatement.Kind)
            {
                case BoundNodeKind.GotoStatement:
                    BoundGotoStatement gotoStatement = (BoundGotoStatement) lastStatement;
                    BasicBlockEdge edge = Connect(currentBlock, labelToBlockMap[gotoStatement.LabelSymbol]);
                    edges.Add(edge);
                    break;
                case BoundNodeKind.ConditionalGotoStatement:
                    BoundConditionalGotoStatement conditionalGotoStatement = (BoundConditionalGotoStatement) lastStatement;
                    BoundExpression negatedCondition = Negate(conditionalGotoStatement.Condition);

                    BoundExpression jumpCondition = conditionalGotoStatement.JumpIf ? conditionalGotoStatement.Condition : negatedCondition;
                    BoundExpression fallThroughCondition = conditionalGotoStatement.JumpIf ? negatedCondition : conditionalGotoStatement.Condition;

                    BasicBlockEdge? jumpEdge = Connect(currentBlock, labelToBlockMap[conditionalGotoStatement.LabelSymbol], jumpCondition);
                    if (jumpEdge is not null)
                    {
                        edges.Add(jumpEdge);
                    }

                    BasicBlockEdge? fallThroughEdge = Connect(currentBlock, nextBlock, fallThroughCondition);
                    if (fallThroughEdge is not null)
                    {
                        edges.Add(fallThroughEdge);
                    }
                    break;
                case BoundNodeKind.ReturnStatement:
                    BasicBlockEdge returnEdge = Connect(currentBlock, endBlock);
                    edges.Add(returnEdge);
                    break;
                default:
                    BasicBlockEdge defaultEdge = Connect(currentBlock, nextBlock);
                    edges.Add(defaultEdge);
                    break;
            }
        }

        private ImmutableArray<BasicBlock> ComputeBlocks(BoundBlockStatement body)
        {
            ImmutableArray<BasicBlock>.Builder blocks = ImmutableArray.CreateBuilder<BasicBlock>();
            ImmutableArray<BoundStatement>.Builder currentBlockStatements = ImmutableArray.CreateBuilder<BoundStatement>();
            foreach (BoundStatement statement in body.Statements)
            {
                switch (statement.Kind)
                {
                    case BoundNodeKind.VariableDeclarationStatement:
                    case BoundNodeKind.ExpressionStatement:
                        currentBlockStatements.Add(statement);
                        break;
                    case BoundNodeKind.LabelStatement:
                        BasicBlock? previousBlock = StartBlock(currentBlockStatements.ToImmutable());

                        if (previousBlock is not null)
                        {
                            blocks.Add(previousBlock);
                        }

                        currentBlockStatements.Clear();
                        currentBlockStatements.Add(statement);
                        break;
                    case BoundNodeKind.GotoStatement:
                    case BoundNodeKind.ConditionalGotoStatement:
                    case BoundNodeKind.ReturnStatement:
                        currentBlockStatements.Add(statement);
                        BasicBlock currentBlock = EndBlock(currentBlockStatements.ToImmutable())!;
                        blocks.Add(currentBlock);
                        currentBlockStatements.Clear();
                        break;
                }
            }

            BasicBlock? lastBlock = EndBlock(currentBlockStatements.ToImmutable());

            if (lastBlock is not null)
            {
                blocks.Add(lastBlock);
            }

            return blocks.ToImmutable();
        }

        private BoundExpression Negate(BoundExpression condition)
        {
            if (condition.Kind is BoundNodeKind.LiteralExpression)
            {
                BoundLiteralExpression literalExpression = (BoundLiteralExpression) condition;
                return new BoundLiteralExpression(!(bool) literalExpression.Value);
            }

            if (condition.Kind is BoundNodeKind.UnaryExpression)
            {
                BoundUnaryExpression unaryExpression = (BoundUnaryExpression) condition;
                if (unaryExpression.Operator.Kind is BoundUnaryOperatorKind.LogicalNegation)
                {
                    return unaryExpression.Operand;
                }
            }

            if (condition.Kind is BoundNodeKind.BinaryExpression)
            {
                BoundBinaryExpression binaryExpression = (BoundBinaryExpression) condition;
                BoundExpression left = binaryExpression.Left;
                BoundExpression right = binaryExpression.Right;

                BoundBinaryOperator? @operator = binaryExpression.Operator.Kind switch
                {
                    BoundBinaryOperatorKind.Equals => BoundBinaryOperator.Bind(SyntaxKind.BangEqualsToken, left.Type, right.Type),
                    BoundBinaryOperatorKind.NotEquals => BoundBinaryOperator.Bind(SyntaxKind.EqualsEqualsToken, left.Type, right.Type),
                    BoundBinaryOperatorKind.LessThan => BoundBinaryOperator.Bind(SyntaxKind.GreaterOrEqualsToken, left.Type, right.Type),
                    BoundBinaryOperatorKind.LessThanOrEquals => BoundBinaryOperator.Bind(SyntaxKind.GreaterToken, left.Type, right.Type),
                    BoundBinaryOperatorKind.GreaterThan => BoundBinaryOperator.Bind(SyntaxKind.LessOrEqualsToken, left.Type, right.Type),
                    BoundBinaryOperatorKind.GreaterThanOrEquals => BoundBinaryOperator.Bind(SyntaxKind.LessToken, left.Type, right.Type),
                    _ => null,
                };

                if (@operator is not null)
                {
                    return new BoundBinaryExpression(left, @operator, right);
                }
            }

            BoundUnaryOperator negationOperator = BoundUnaryOperator.Bind(SyntaxKind.BangToken, condition.Type)!;
            return new BoundUnaryExpression(negationOperator, condition);
        }

        private ImmutableDictionary<BoundLabel, BasicBlock> IndexLabelsForBlocks(ImmutableArray<BasicBlock> blocks)
        {
            ImmutableDictionary<BoundLabel, BasicBlock>.Builder labelToBlockMap = ImmutableDictionary.CreateBuilder<BoundLabel, BasicBlock>();
            foreach (BasicBlock block in blocks)
            {
                if (block.Statements.Length > 0 && block.Statements[0].Kind is BoundNodeKind.LabelStatement)
                {
                    BoundLabelStatement labelStatement = (BoundLabelStatement) block.Statements[0];
                    labelToBlockMap[labelStatement.LabelSymbol] = block;
                }
            }

            return labelToBlockMap.ToImmutable();
        }

        private BasicBlockEdge Connect(BasicBlock fromBlock, BasicBlock toBlock)
        {

            BasicBlockEdge edge = new(fromBlock, toBlock, null);
            _blockOutgoingEdges[fromBlock].Add(edge);
            _blockIncomingEdges[toBlock].Add(edge);

            return edge;
        }

        private BasicBlockEdge? Connect(BasicBlock fromBlock, BasicBlock toBlock, BoundExpression condition)
        {
            if (condition.Kind is BoundNodeKind.LiteralExpression)
            {
                BoundLiteralExpression literalCondition = (BoundLiteralExpression) condition;
                if ((bool) literalCondition.Value)
                {
                    return Connect(fromBlock, toBlock);
                }
                else
                {
                    return null;
                }
            }

            BasicBlockEdge edge = new(fromBlock, toBlock, condition);
            _blockOutgoingEdges[fromBlock].Add(edge);
            _blockIncomingEdges[toBlock].Add(edge);

            return edge;
        }

        private BasicBlock CreateBlock(ImmutableArray<BoundStatement> statements)
        {
            ImmutableArray<BasicBlockEdge>.Builder incoming = ImmutableArray.CreateBuilder<BasicBlockEdge>();
            ImmutableArray<BasicBlockEdge>.Builder outgoing = ImmutableArray.CreateBuilder<BasicBlockEdge>();

            BasicBlock block = new(
                statements,
                incoming.ToImmutable,
                outgoing.ToImmutable);

            _blockIncomingEdges[block] = incoming;
            _blockOutgoingEdges[block] = outgoing;

            return block;
        }

        private BasicBlock? StartBlock(ImmutableArray<BoundStatement> previousBlockStatements)
        {
            return EndBlock(previousBlockStatements);
        }

        private BasicBlock? EndBlock(ImmutableArray<BoundStatement> currentBlockStatements)
        {
            if (currentBlockStatements.Length > 0)
            {
                BasicBlock block = CreateBlock(currentBlockStatements);
                return block;
            }

            return null;
        }
    }
}
