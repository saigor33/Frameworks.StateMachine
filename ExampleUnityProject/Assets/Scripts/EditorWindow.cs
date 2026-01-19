using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Frameworks.StateMachine.StateGraphVisualizer
{
    public class EditorWindow : UnityEditor.EditorWindow
    {
        class EnumOption
        {
            public int selectedIndex;
            public Type[] types;
            public string[] typeFullNames;
        }

        Type[] _assemblyTypes;
        bool _needVisualizeTransitions;
        EnumOption _inheritBaseStateEnumOption;
        string _sateGraphText;
        string _sateGraphWithTransitionsText;
        Vector2 _generationResultScrollPosition;
        string _sourceCodeDirectoryPath;

        // [MenuItem("Tools/StateMachine/Visualization")]
        [MenuItem("Tools/StateMachineVisualization")]
        public static void ShowWindow()
        {
            GetWindow<EditorWindow>();
        }

        void OnEnable()
        {
            Type baseStateType = typeof(BaseState<>);
            Type baseTransitionType = typeof(BaseTransition<>); // remove select transition?
            Type baseTransitionWithContextType = typeof(BaseTransition<,>);

            Type[] assemblyTypes = AppDomain
               .CurrentDomain
               .GetAssemblies()
               .SelectMany(assembly => assembly.GetTypes())
               .ToArray();
            Type[] allAbstractClassTypes = assemblyTypes
               .Where(type => type.IsClass && type.IsAbstract)
               .ToArray();

            _assemblyTypes = assemblyTypes;

            Type[] inheritBaseStateTypes = TypesHelpers.GetInheritGenericTypes(allAbstractClassTypes, baseStateType);
            _inheritBaseStateEnumOption = new EnumOption
            {
                selectedIndex = 0, // todo: array can be empty
                types = inheritBaseStateTypes,
                typeFullNames = inheritBaseStateTypes
                   .Select(t => t.FullName)
                   .ToArray()
            };
            _needVisualizeTransitions = true;
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Space(5);

            DrawSelectFolderPath();

            GUILayout.Space(5);

            DrawSelectionState();

            // todo: add option "not all select transition"
            // select transition
            // select typed transition
            GUILayout.Space(5);

            _needVisualizeTransitions = EditorGUILayout.Toggle("Need visualize transitions", _needVisualizeTransitions);

            GUILayout.Space(5);
            if (GUILayout.Button("Generate"))
            {
                GenerateGraphvizCode();
            }

            GUILayout.Space(5);
            DrawGenerationResult();

            GUILayout.EndVertical();
        }

        void DrawSelectFolderPath()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("Select source code directory");

            if (GUILayout.Button("Select", GUILayout.Width(100)))
            {
                _sourceCodeDirectoryPath =
                    EditorUtility.OpenFolderPanel("Select source code folder", Application.dataPath, "");
            }

            EditorGUI.BeginDisabledGroup(true);
            GUILayout.TextField(_sourceCodeDirectoryPath, GUILayout.MinWidth(500), GUILayout.ExpandWidth(true));
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        void DrawGenerationResult()
        {
            _generationResultScrollPosition = GUILayout.BeginScrollView(_generationResultScrollPosition
            );

            GUILayout.BeginHorizontal();

            GUILayoutOption[] layoutOptions = { GUILayout.MaxWidth(maxSize.x / 2) };

            _sateGraphText = GUILayout.TextArea(_sateGraphText, layoutOptions);

            if (_needVisualizeTransitions)
            {
                _sateGraphWithTransitionsText = GUILayout.TextArea(_sateGraphWithTransitionsText, layoutOptions);
            }

            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
        }

        void DrawSelectionState()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("BaseState type");

            _inheritBaseStateEnumOption.selectedIndex = EditorGUILayout.Popup(
                _inheritBaseStateEnumOption.selectedIndex,
                _inheritBaseStateEnumOption.typeFullNames
            );

            GUILayout.EndHorizontal();
        }

        void GenerateGraphvizCode()
        {
            Type selectedBaseStateType =
                _inheritBaseStateEnumOption.types[_inheritBaseStateEnumOption.selectedIndex];
            Type[] inheritSelectedBaseStateTypes =
                TypesHelpers.GetInheritTypes(_assemblyTypes, selectedBaseStateType);

            Type[] inheritSelectedTransitionTypes = new[]
                {
                    TypesHelpers.GetInheritTypes(_assemblyTypes, typeof(Match.Logic.BaseTransition)),
                    TypesHelpers.GetInheritGenericTypes(_assemblyTypes, typeof(Match.Logic.BaseTransition<>))
                }
               .SelectMany(t => t)
               .ToArray();

            CodeAnalyzer.Result codeAnalyzeResult =
                CodeAnalyzer.Analyze(inheritSelectedBaseStateTypes, inheritSelectedTransitionTypes);

            // generate text

            string sateGraphText = string.Join("\n",
                codeAnalyzeResult.fromStateToTransitionByTransition.Select(kv =>
                {
                    (string transitionId, HashSet<string> stateIds) = kv;
                    return string.Join("\n", stateIds.Select(stateId => $"{stateId} -> {transitionId}"));
                }));

            StateGraphBuild.Result stateGraphResult = StateGraphBuild.Build(codeAnalyzeResult);

            _sateGraphText = sateGraphText;
            _sateGraphWithTransitionsText = stateGraphResult.stateGraphWithTransitionsGraphvizText;
        }
    }

    static class GraphvizFormater
    {
        public static class ShapeType
        {
            public const string Rect = "rect";
            public const string Ellipse = "ellipse";
        }

        public static class Color
        {
            public const string Lightgrey = "lightgrey";
            public const string White = "white";
            public const string Red = "red";
            public const string Yellow = "yellow";
        }

        public static class Style
        {
            public const string Filled = "filled";
        }

        public static string FormatDigraph(string id, string label, string nodes)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("digraph G");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(FormateLineIndentation($"label = \"{label}\""));
            stringBuilder.AppendLine(FormateLineIndentation(nodes));
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        public static string FormatSubgraph(string label, string nodes, string style = Style.Filled,
            string color = Color.Lightgrey)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"subgraph cluster_{label}");
            stringBuilder.AppendLine("{");
            stringBuilder.AppendLine(FormateLineIndentation($"style={style}"));
            stringBuilder.AppendLine(FormateLineIndentation($"color={color}"));
            stringBuilder.AppendLine(FormateLineIndentation($"label={label}"));
            stringBuilder.AppendLine(FormateLineIndentation(nodes));
            stringBuilder.AppendLine("}");

            return stringBuilder.ToString();
        }

        public static string FormatNode(string nodeId, string nodeLabel, string shapeType = ShapeType.Ellipse,
            string color = Color.White, string style = Style.Filled)
        {
            return $"\"{nodeId}\" [label=\"{nodeLabel}\" shape=\"{shapeType}\" color=\"{color}\" style=\"{style}\"]";
        }

        public static string JoinNodes(string fromNodeId, string toNodeId)
        {
            return $"\"{fromNodeId}\" -> \"{toNodeId}\"";
        }

        static string FormateLineIndentation(string text)
        {
            return $"\t{text.Replace("\n", "\n\t")}";
        }
    }

    static class StateGraphBuild
    {
        public class Result
        {
            public string stateGraphGraphvizText;
            public string stateGraphWithTransitionsGraphvizText;
        }

        public static Result Build(CodeAnalyzer.Result codeAnalyzer)
        {
            string commonSubname = GetCommonSubstring(codeAnalyzer.fromStateToTransitionByTransition);
            commonSubname = GetCommonSubstring(codeAnalyzer.fromTransitionToStateByState, commonSubname);
            commonSubname = GetCommonSubstring(codeAnalyzer.fromOtherSourceToTransitionByTransition, commonSubname);
            commonSubname = GetCommonSubstring(codeAnalyzer.fromOtherSourceToStateByState, commonSubname);

            // state to state graph
            // state to transition graph

            var stringBuilder = new StringBuilder();

            var fromStateToTransitionByState = new Dictionary<string, HashSet<string>>();
            foreach ((string transitionId, HashSet<string> stateIds) in codeAnalyzer.fromStateToTransitionByTransition)
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

            var createdTransitionIds = new HashSet<string>();
            foreach ((string stateId, HashSet<string> transitionIds) in fromStateToTransitionByState)
            {
                createdTransitionIds.UnionWith(transitionIds);
                stringBuilder.AppendLine(CreateStateWithTransitionsNode(stateId, transitionIds));
            }

            foreach (string stateId in codeAnalyzer.states)
            {
                bool isStateNodeCreated = fromStateToTransitionByState.ContainsKey(stateId);
                if (!isStateNodeCreated)
                {
                    string notUsedStateNode =
                        CreateStateWithTransitionsNode(stateId, transitionIds: new HashSet<string>());
                    stringBuilder.AppendLine(notUsedStateNode);
                }
            }

            foreach (string transitionId in codeAnalyzer.transitions)
            {
                bool isTransitionNodeCreated = createdTransitionIds.Contains(transitionId);
                if (!isTransitionNodeCreated)
                {
                    string notUsedTransitionNode = CreateTransitionNode(transitionId, GraphvizFormater.Color.Yellow);
                    stringBuilder.AppendLine(notUsedTransitionNode);
                }
            }

            foreach ((string stateId, HashSet<string> transitionIds) in codeAnalyzer.fromTransitionToStateByState)
            {
                foreach (string transitionId in transitionIds)
                {
                    stringBuilder.AppendLine(GraphvizFormater.JoinNodes(transitionId, stateId));
                }
            }

            HashSet<string> allOtherSourceIds = new Dictionary<string, HashSet<string>>()
               .Concat(codeAnalyzer.fromOtherSourceToStateByState)
               .Concat(codeAnalyzer.fromOtherSourceToTransitionByTransition)
               .SelectMany(kv => kv.Value)
               .ToHashSet();

            foreach (string otherSourceId in allOtherSourceIds)
            {
                stringBuilder.AppendLine(
                    GraphvizFormater.FormatNode(
                        nodeId: otherSourceId,
                        nodeLabel: GetSourceName(otherSourceId),
                        color: GraphvizFormater.Color.Yellow
                    ));
            }

            foreach ((string stateId, HashSet<string> otherSourceIds) in codeAnalyzer.fromOtherSourceToStateByState)
            {
                foreach (string otherSourceId in otherSourceIds)
                {
                    stringBuilder.Append(GraphvizFormater.JoinNodes(otherSourceId, stateId));
                }
            }

            foreach ((string transitionId, HashSet<string> otherSourceIds) in codeAnalyzer
               .fromOtherSourceToTransitionByTransition)
            {
                foreach (string otherSourceId in otherSourceIds)
                {
                    stringBuilder.Append(GraphvizFormater.JoinNodes(otherSourceId, transitionId));
                }
            }

            return new Result
            {
                stateGraphGraphvizText = "",
                stateGraphWithTransitionsGraphvizText =
                    GraphvizFormater.FormatDigraph("Root", "", stringBuilder.ToString())
            };
        }

        static string CreateStateWithTransitionsNode(string stateId, HashSet<string> transitionIds)
        {
            var stateStringBuild = new StringBuilder();

            stateStringBuild.AppendLine(GraphvizFormater.FormatNode(stateId, "State"));

            foreach (string transitionId in transitionIds)
            {
                stateStringBuild.AppendLine(CreateTransitionNode(transitionId));
                stateStringBuild.AppendLine(GraphvizFormater.JoinNodes(stateId, transitionId));
            }

            return GraphvizFormater.FormatSubgraph(GetSourceName(stateId), nodes: $"{stateStringBuild}");
        }

        static string CreateTransitionNode(string transitionId, string color = GraphvizFormater.Color.White)
        {
            return GraphvizFormater.FormatNode(transitionId,
                nodeLabel: GetSourceName(transitionId),
                color: color,
                shapeType: GraphvizFormater.ShapeType.Rect);
        }

        static string GetSourceName(string sourceId)
        {
            int lastIndexOf = sourceId.LastIndexOf('.');

            if (lastIndexOf == -1)
            {
                return sourceId;
            }

            return sourceId.Substring(lastIndexOf + 1);
        }

        static string GetCommonSubstring(Dictionary<string, HashSet<string>> allSourceToSources,
            string commonSubname = null)
        {
            foreach ((string fromSourceId, HashSet<string> toSourceIds) in allSourceToSources)
            {
                commonSubname = commonSubname == null
                    ? fromSourceId
                    : GetCommonSubstring(commonSubname, fromSourceId);

                foreach (string toSourceId in toSourceIds)
                {
                    commonSubname = GetCommonSubstring(commonSubname, toSourceId);
                }
            }

            return commonSubname;
        }

        static string GetCommonSubstring(string str1, string str2)
        {
            int? lastMatchSubstringIndex = null;
            for (int i = 0; i < str1.Length; i++)
            {
                if (i >= str2.Length
                    || str1[i] != str2[i])
                {
                    break;
                }

                lastMatchSubstringIndex = i;
            }

            int commonSubstringLenght = lastMatchSubstringIndex.HasValue
                ? lastMatchSubstringIndex.Value + 1
                : 0;

            return str1.Substring(0, commonSubstringLenght);
        }
    }
}