using System;
using System.Collections.Generic;
using System.Linq;
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

            HashSet<string> allSourceIds = new Dictionary<string, HashSet<string>>()
               .Concat(codeAnalyzeResult.fromStateToTransitionByTransition)
               .Concat(codeAnalyzeResult.fromTransitionToStateByState)
               .Concat(codeAnalyzeResult.fromOtherSourceToTransitionByTransition)
               .Concat(codeAnalyzeResult.fromOtherSourceToStateByState)
               .SelectMany(kv => new HashSet<string>(kv.Value.Union(new[] { kv.Key })))
               .ToHashSet();

            string stateGraphName = CommonSubstringFinder
               .GetCommonSubstring(allSourceIds)
               .TrimEnd('.');

            string stateGraphDescription = $"FeatureName: {stateGraphName}";

            GraphvizStateGraphGenerator.Result stateGraphResult = GraphvizStateGraphGenerator.Build(codeAnalyzeResult);

            _sateGraphText = Graphviz.StateGraphGenerator.GenerateStateGraph(codeAnalyzeResult, stateGraphDescription);
            _sateGraphWithTransitionsText = stateGraphResult.stateGraphWithTransitionsText;
        }
    }
}