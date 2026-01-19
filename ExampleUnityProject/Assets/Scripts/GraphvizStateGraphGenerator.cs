using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frameworks.StateMachine.StateGraphVisualizer
{
    static class GraphvizStateGraphGenerator
    {
        public class Result
        {
            public string stateGraphText;
            public string stateGraphWithTransitionsText;
        }

        public static Result Build(CodeAnalyzer.Result codeAnalyzeResult)
        {

            var fromStateToTransitionByState = new Dictionary<string, HashSet<string>>();
            foreach ((string transitionId, HashSet<string> stateIds) in codeAnalyzeResult
               .fromStateToTransitionByTransition)
            {
                foreach (string stateId in stateIds)
                {
                    if (fromStateToTransitionByState.TryGetValue(stateId, out HashSet<string> value))
                    {
                        value.Add(transitionId);
                    }
                    else
                    {
                        fromStateToTransitionByState.Add(stateId, new HashSet<string>(new[] { transitionId }));
                    }
                }
            }

            var stringBuilder = new StringBuilder();

            var createdTransitionIds = new HashSet<string>();
            foreach ((string stateId, HashSet<string> transitionIds) in fromStateToTransitionByState)
            {
                createdTransitionIds.UnionWith(transitionIds);
                stringBuilder.AppendLine(CreateStateWithTransitionsNode(stateId, transitionIds));
            }

            foreach (string stateId in codeAnalyzeResult.states)
            {
                bool isStateNodeCreated = fromStateToTransitionByState.ContainsKey(stateId);
                if (!isStateNodeCreated)
                {
                    string notUsedStateNode =
                        CreateStateWithTransitionsNode(stateId, transitionIds: new HashSet<string>());
                    stringBuilder.AppendLine(notUsedStateNode);
                }
            }

            foreach (string transitionId in codeAnalyzeResult.transitions)
            {
                bool isTransitionNodeCreated = createdTransitionIds.Contains(transitionId);
                if (!isTransitionNodeCreated)
                {
                    string notUsedTransitionNode = CreateTransitionNode(transitionId, GraphvizFormatter.Color.Yellow);
                    stringBuilder.AppendLine(notUsedTransitionNode);
                }
            }

            foreach ((string stateId, HashSet<string> transitionIds) in codeAnalyzeResult.fromTransitionToStateByState)
            {
                foreach (string transitionId in transitionIds)
                {
                    stringBuilder.AppendLine(GraphvizFormatter.JoinNodes(transitionId, stateId));
                }
            }

            HashSet<string> allOtherSourceIds = new Dictionary<string, HashSet<string>>()
               .Concat(codeAnalyzeResult.fromOtherSourceToStateByState)
               .Concat(codeAnalyzeResult.fromOtherSourceToTransitionByTransition)
               .SelectMany(kv => kv.Value)
               .ToHashSet();

            foreach (string otherSourceId in allOtherSourceIds)
            {
                stringBuilder.AppendLine(
                    GraphvizFormatter.FormatNode(
                        nodeId: otherSourceId,
                        nodeLabel: SourceNameHelper.GetSourceName(otherSourceId),
                        color: GraphvizFormatter.Color.Yellow
                    ));
            }

            foreach ((string stateId, HashSet<string> otherSourceIds) in
                codeAnalyzeResult.fromOtherSourceToStateByState)
            {
                foreach (string otherSourceId in otherSourceIds)
                {
                    stringBuilder.Append(GraphvizFormatter.JoinNodes(otherSourceId, stateId));
                }
            }

            foreach ((string transitionId, HashSet<string> otherSourceIds) in codeAnalyzeResult
               .fromOtherSourceToTransitionByTransition)
            {
                foreach (string otherSourceId in otherSourceIds)
                {
                    stringBuilder.Append(GraphvizFormatter.JoinNodes(otherSourceId, transitionId));
                }
            }

            return new Result
            {
                stateGraphText = Graphviz.StateGraphGenerator.GenerateStateGraph(codeAnalyzeResult),
                stateGraphWithTransitionsText =
                    GraphvizFormatter.FormatDigraph("Root", "", stringBuilder.ToString())
            };
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

            return GraphvizFormatter.FormatSubgraph(SourceNameHelper.GetSourceName(stateId),
                nodes: $"{stateStringBuild}");
        }

        static string CreateTransitionNode(string transitionId, string color = GraphvizFormatter.Color.White)
        {
            return GraphvizFormatter.FormatNode(transitionId,
                nodeLabel: SourceNameHelper.GetSourceName(transitionId),
                color: color,
                shapeType: GraphvizFormatter.ShapeType.Rect);
        }
    }
}