using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frameworks.StateMachine.StateGraphVisualizer.Graphviz
{
    static class StateGraphWithTransitionsGenerator
    {
        public static string Generate(CodeAnalyzer.Result codeAnalyzeResult, string label)
        {
            HashSet<string> stateIds = codeAnalyzeResult.states;
            HashSet<string> transitionIds = codeAnalyzeResult.transitions;
            Dictionary<string, HashSet<string>> fromStateToTransitionByTransition = codeAnalyzeResult
               .fromStateToTransitionByTransition;
            Dictionary<string, HashSet<string>> fromOtherSourceToTransitionByTransition = codeAnalyzeResult
               .fromOtherSourceToTransitionByTransition;
            Dictionary<string, HashSet<string>> fromTransitionToStateByState =
                codeAnalyzeResult.fromTransitionToStateByState;
            Dictionary<string, HashSet<string>> fromOtherSourceToStateByState =
                codeAnalyzeResult.fromOtherSourceToStateByState;

            var fromStateToTransitionByState = new Dictionary<string, HashSet<string>>();
            foreach ((string toTransitionId, HashSet<string> fromStateIds) in fromStateToTransitionByTransition)
            {
                foreach (string fromStateId in fromStateIds)
                {
                    if (fromStateToTransitionByState.TryGetValue(fromStateId, out HashSet<string> value))
                    {
                        value.Add(toTransitionId);
                    }
                    else
                    {
                        fromStateToTransitionByState.Add(fromStateId, new HashSet<string>(new[] { toTransitionId }));
                    }
                }
            }

            var stringBuilder = new StringBuilder();

            var createdTransitionIds = new HashSet<string>();
            foreach ((string fromStateId, HashSet<string> toTransitionIds) in fromStateToTransitionByState)
            {
                createdTransitionIds.UnionWith(toTransitionIds);
                stringBuilder.AppendLine(CreateStateWithTransitionsNode(fromStateId, toTransitionIds));
            }

            foreach (string stateId in stateIds)
            {
                bool isStateNodeCreated = fromStateToTransitionByState.ContainsKey(stateId);
                if (!isStateNodeCreated)
                {
                    string notUsedStateNode =
                        CreateStateWithTransitionsNode(stateId, transitionIds: new HashSet<string>());
                    stringBuilder.AppendLine(notUsedStateNode);
                }
            }

            foreach (string transitionId in transitionIds)
            {
                bool isTransitionNodeCreated = createdTransitionIds.Contains(transitionId);
                if (!isTransitionNodeCreated)
                {
                    string notUsedTransitionNode = CreateTransitionNode(transitionId, GraphvizFormatter.Color.Yellow);
                    stringBuilder.AppendLine(notUsedTransitionNode);
                }
            }

            foreach ((string toStateId, HashSet<string> fromTransitionIds) in fromTransitionToStateByState)
            {
                foreach (string fromTransitionId in fromTransitionIds)
                {
                    stringBuilder.AppendLine(GraphvizFormatter.JoinNodes(fromTransitionId, toStateId));
                }
            }

            HashSet<string> allOtherSourceIds = new Dictionary<string, HashSet<string>>()
               .Concat(fromOtherSourceToStateByState)
               .Concat(fromOtherSourceToTransitionByTransition)
               .SelectMany(kv => kv.Value)
               .ToHashSet();

            foreach (string otherSourceId in allOtherSourceIds)
            {
                stringBuilder.AppendLine(
                    GraphvizFormatter.FormatNode(
                        nodeId: otherSourceId,
                        nodeLabel: SourceNameHelper.GetSourceName(otherSourceId),
                        color: GraphvizFormatter.Color.Yellow
                    )
                );
            }

            foreach ((string toStateId, HashSet<string> fromOtherSourceIds) in
                fromOtherSourceToStateByState)
            {
                foreach (string fromOtherSourceId in fromOtherSourceIds)
                {
                    stringBuilder.Append(GraphvizFormatter.JoinNodes(fromOtherSourceId, toStateId));
                }
            }

            foreach ((string toTransitionId, HashSet<string> fromOtherSourceIds) in
                fromOtherSourceToTransitionByTransition)
            {
                foreach (string fromOtherSourceId in fromOtherSourceIds)
                {
                    stringBuilder.Append(GraphvizFormatter.JoinNodes(fromOtherSourceId, toTransitionId));
                }
            }

            string withTitleContainerNode =
                GraphvizFormatter.FormatSubgraph("Container", label, nodes: $"{stringBuilder}");
            return GraphvizFormatter.FormatDigraph(id: "Root", "", withTitleContainerNode);
        }

        static string CreateStateWithTransitionsNode(string stateId, HashSet<string> transitionIds)
        {
            var stateStringBuild = new StringBuilder();

            stateStringBuild.AppendLine(GraphvizFormatter.FormatNode(stateId, "State"));

            foreach (string transitionId in transitionIds)
            {
                stateStringBuild.AppendLine(CreateTransitionNode(transitionId));
                stateStringBuild.AppendLine(GraphvizFormatter.JoinNodes(stateId, transitionId));
            }

            string stateName = SourceNameHelper.GetSourceName(stateId);

            return GraphvizFormatter.FormatSubgraph(
                id: stateName,
                label: stateName,
                nodes: $"{stateStringBuild}"
            );
        }

        static string CreateTransitionNode(string transitionId, string color = GraphvizFormatter.Color.White)
        {
            return GraphvizFormatter.FormatNode(
                transitionId,
                nodeLabel: SourceNameHelper.GetSourceName(transitionId),
                color: color,
                shapeType: GraphvizFormatter.ShapeType.Rect
            );
        }
    }
}