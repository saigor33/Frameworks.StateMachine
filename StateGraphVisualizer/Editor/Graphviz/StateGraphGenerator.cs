using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frameworks.StateMachine.StateGraphVisualizer.Graphviz
{
    static class StateGraphGenerator
    {
        public static string GenerateStateGraph(CodeAnalyzer.Result codeAnalyzeResult, string label)
        {
            HashSet<string> stateIds = codeAnalyzeResult.states;
            Dictionary<string, HashSet<string>> fromTransitionToStateByState =
                codeAnalyzeResult.fromTransitionToStateByState;
            Dictionary<string, HashSet<string>> fromStateToTransitionByTransition =
                codeAnalyzeResult.fromStateToTransitionByTransition;
            Dictionary<string, HashSet<string>> fromOtherSourceToStateByState =
                codeAnalyzeResult.fromOtherSourceToStateByState;

            var stringBuilder = new StringBuilder();

            foreach (string stateId in stateIds)
            {
                stringBuilder.AppendLine(
                    GraphvizFormatter.FormatNode(
                        nodeId: stateId,
                        color: GraphvizFormatter.Color.Lightgrey,
                        nodeLabel: SourceNameHelper.GetSourceName(stateId)
                    )
                );
            }

            foreach ((string toStateId, HashSet<string> fromTransitionIds) in fromTransitionToStateByState)
            {
                foreach (string fromTransitionId in fromTransitionIds)
                {
                    if (fromStateToTransitionByTransition.TryGetValue(fromTransitionId,
                        out HashSet<string> fromStateIds))
                    {
                        foreach (string fromStateId in fromStateIds)
                        {
                            stringBuilder.AppendLine(GraphvizFormatter.JoinNodes(fromNodeId: fromStateId,
                                toNodeId: toStateId));
                        }
                    }
                }
            }

            HashSet<string> toStateOtherSourceIds = fromOtherSourceToStateByState
               .SelectMany(kv => kv.Value)
               .ToHashSet();

            foreach (string toStateOtherSourceId in toStateOtherSourceIds)
            {
                stringBuilder.AppendLine(
                    GraphvizFormatter.FormatNode(
                        nodeId: toStateOtherSourceId,
                        nodeLabel: SourceNameHelper.GetSourceName(toStateOtherSourceId),
                        color: GraphvizFormatter.Color.Yellow
                    )
                );
            }

            foreach ((string stateId, HashSet<string> otherSourceIds) in fromOtherSourceToStateByState)
            {
                foreach (string otherSourceId in otherSourceIds)
                {
                    stringBuilder.AppendLine(GraphvizFormatter.JoinNodes(fromNodeId: otherSourceId,
                        toNodeId: stateId));
                }
            }

            string withTitleContainerNode =
                GraphvizFormatter.FormatSubgraph("Container", label, nodes: $"{stringBuilder}");
            return GraphvizFormatter.FormatDigraph(id: "Root", "", withTitleContainerNode);
        }
    }
}