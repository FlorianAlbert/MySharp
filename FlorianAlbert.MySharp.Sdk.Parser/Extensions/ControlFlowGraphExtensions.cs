using FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;
using System.CodeDom.Compiler;

namespace FlorianAlbert.MySharp.Sdk.Parser.Extensions;

internal static class ControlFlowGraphExtensions
{
    extension(ControlFlowGraph controlFlowGraph)
    {
        public void WriteGraphVizTo(TextWriter textWriter)
        {
            textWriter.WriteLine("digraph ControlFlowGraph {");

            Dictionary<ControlFlowGraph.BasicBlock, string> blockIds = [];
            for (int blockIndex = 0; blockIndex < controlFlowGraph.Blocks.Length; blockIndex++)
            {
                ControlFlowGraph.BasicBlock block = controlFlowGraph.Blocks[blockIndex];
                string blockId = $"Block{blockIndex}";
                blockIds[block] = blockId;

                if (blockIndex == 0)
                {
                    textWriter.WriteLine($"    {blockId} [label = \"<Start>\", shape = box];");
                }
                else if (blockIndex == controlFlowGraph.Blocks.Length - 1)
                {
                    textWriter.WriteLine($"    {blockId} [label = \"<End>\", shape = box];");
                }
                else
                {
                    textWriter.WriteLine($"    {blockId} [label = \"{WriteBlockStatements(block)}\", shape = box];");
                }
            }

            for (int edgeIndex = 0; edgeIndex < controlFlowGraph.Edges.Length; edgeIndex++)
            {
                ControlFlowGraph.BasicBlockEdge edge = controlFlowGraph.Edges[edgeIndex];
                string fromBlockId = blockIds[edge.FromBlock];
                string toBlockId = blockIds[edge.ToBlock];

                string label = edge.Condition is not null ? $" [label=\"{edge.Condition}\"]" : string.Empty;

                textWriter.WriteLine($"    {fromBlockId} -> {toBlockId}{label};");
            }

            textWriter.WriteLine("}");
        }
    }

    private static string WriteBlockStatements(ControlFlowGraph.BasicBlock block)
    {
        using StringWriter stringWriter = new();
        using IndentedTextWriter indentedTextWriter = new(stringWriter);
        foreach (BoundStatement statement in block.Statements)
        {
            statement.WriteTo(indentedTextWriter);
        }

        string escapedStatements = stringWriter.ToString()
                                               .Replace("\\", "\\\\")
                                               .Replace("\"", "\\\"")
                                               .Replace(Environment.NewLine, "\\l");
        return escapedStatements;
    }
}
