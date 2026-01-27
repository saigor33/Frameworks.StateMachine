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
            public bool enabled;
            public int selectedIndex;
            public Type[] types;
            public string[] typeFullNames;
        }

        static readonly string[] GraphvizVisualEditorUrls =
        {
            "https://www.devtoolsdaily.com/graphviz",
            "https://graph.flyte.org"
        };

        Type[] _assemblyTypes;
        bool _needVisualizeTransitions;
        EnumOption _inheritBaseStateEnumOption;
        EnumOption _inheritBaseTransitionEnumOption;
        EnumOption _inheritBaseTransitionWithContextEnumOption;
        string _sateGraphText;
        string _sateGraphWithTransitionsText;
        Vector2 _generationResultScrollPosition;
        string _sourceCodeDirectoryPath;

        [MenuItem("Tools/StateMachine/Visualization")]
        public static void ShowWindow()
        {
            GetWindow<EditorWindow>();
        }

        void OnEnable()
        {
            Type baseStateType = typeof(BaseState<>);
            Type baseTransitionType = typeof(BaseTransition<>);
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
            _sourceCodeDirectoryPath = Application.dataPath;

            Type[] inheritBaseStateTypes = TypesHelpers.GetInheritGenericTypes(allAbstractClassTypes, baseStateType);
            _inheritBaseStateEnumOption = new EnumOption
            {
                enabled = true,
                selectedIndex = 0,
                types = inheritBaseStateTypes,
                typeFullNames = inheritBaseStateTypes
                   .Select(t => t.FullName)
                   .ToArray()
            };

            Type[] inheritBaseTransitionTypes =
                TypesHelpers.GetInheritGenericTypes(allAbstractClassTypes, baseTransitionType);
            _inheritBaseTransitionEnumOption = new EnumOption
            {
                enabled = inheritBaseTransitionTypes.Length > 0,
                selectedIndex = 0,
                types = inheritBaseTransitionTypes,
                typeFullNames = inheritBaseTransitionTypes
                   .Select(t => t.FullName)
                   .ToArray()
            };

            Type[] inheritBaseTransitionWithContextTypes = TypesHelpers.GetInheritGenericTypes(
                allAbstractClassTypes,
                baseTransitionWithContextType
            );
            _inheritBaseTransitionWithContextEnumOption = new EnumOption
            {
                enabled = inheritBaseTransitionWithContextTypes.Length > 0,
                selectedIndex = 0,
                types = inheritBaseTransitionWithContextTypes,
                typeFullNames = inheritBaseTransitionWithContextTypes
                   .Select(t => t.FullName)
                   .ToArray()
            };
            _needVisualizeTransitions = true;
        }

        void OnGUI()
        {
            if (_inheritBaseTransitionEnumOption.types.Length < 1)
            {
                GUILayout.Label("BaseState implementation not exists. Create class with inherit BaseState type.");
                return;
            }

            GUILayout.BeginVertical();
            GUILayout.Space(5);

            DrawSelectionType("BaseState type", _inheritBaseStateEnumOption);
            DrawSelectionType("BaseTransition type", _inheritBaseTransitionEnumOption);
            DrawSelectionType("BaseTransitionWithContext type", _inheritBaseTransitionWithContextEnumOption);

            GUILayout.Space(5);
            DrawSelectSourceCodeDirectoryPath();

            GUILayout.Space(5);
            _needVisualizeTransitions = EditorGUILayout.Toggle("Need visualize transitions", _needVisualizeTransitions);

            GUILayout.Space(5);
            if (GUILayout.Button("Generate"))
            {
                GenerateGraphvizCode();
            }

            GUILayout.Space(5);
            DrawGenerationResult();

            GUILayout.Space(5);
            DrawVisualEditorsToShowing();

            GUILayout.EndVertical();
        }

        void DrawSelectionType(string labelText, EnumOption enumOption)
        {
            GUILayout.BeginHorizontal();

            enumOption.enabled = EditorGUILayout.Toggle("", enumOption.enabled, GUILayout.Width(15));

            EditorGUI.BeginDisabledGroup(!enumOption.enabled);
            GUILayout.Label(labelText, GUILayout.Width(400));
            if (enumOption.enabled)
            {
                enumOption.selectedIndex = EditorGUILayout.Popup(enumOption.selectedIndex, enumOption.typeFullNames);
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();
        }

        void DrawSelectSourceCodeDirectoryPath()
        {
            GUILayout.BeginHorizontal();

            if (string.IsNullOrEmpty(_sourceCodeDirectoryPath))
            {
                GUILayout.Label("⚠️");
            }

            GUILayout.Label("Select source code directory");

            if (GUILayout.Button("Select", GUILayout.Width(100)))
            {
                string panelTitle = "Select source code folder";
                _sourceCodeDirectoryPath = EditorUtility.OpenFolderPanel(panelTitle, Application.dataPath, "");
            }

            EditorGUI.BeginDisabledGroup(true);
            GUILayout.TextField(_sourceCodeDirectoryPath, GUILayout.MinWidth(500), GUILayout.ExpandWidth(true));
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            GUILayout.EndHorizontal();
        }

        void DrawGenerationResult()
        {
            _generationResultScrollPosition = GUILayout.BeginScrollView(_generationResultScrollPosition);

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

        void DrawVisualEditorsToShowing()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Visual editors to showing graphviz code:");

            foreach (string graphvizVisualEditorUrl in GraphvizVisualEditorUrls)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(graphvizVisualEditorUrl, GUILayout.Width(250));
                if (GUILayout.Button("Open", GUILayout.Width(50)))
                {
                    Application.OpenURL(graphvizVisualEditorUrl);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        void GenerateGraphvizCode()
        {
            static Type GetSelectedType(EnumOption enumOption) => enumOption.types[enumOption.selectedIndex];

            if (string.IsNullOrEmpty(_sourceCodeDirectoryPath))
            {
                Debug.LogWarning("Source code directory path is empty.");
                return;
            }

            Type selectedBaseStateType = GetSelectedType(_inheritBaseStateEnumOption);
            Type[] inheritSelectedBaseStateTypes = TypesHelpers.GetInheritTypes(_assemblyTypes, selectedBaseStateType);

            var inheritSelectedTransitionTypes = new List<Type>();

            if (_inheritBaseTransitionEnumOption.enabled)
            {
                Type selectedBaseTransitionType = GetSelectedType(_inheritBaseTransitionEnumOption);
                Type[] inheritTypes = TypesHelpers.GetInheritTypes(_assemblyTypes, selectedBaseTransitionType);
                inheritSelectedTransitionTypes.AddRange(inheritTypes);
            }

            if (_inheritBaseTransitionWithContextEnumOption.enabled)
            {
                Type selectedBaseTransitionWithContextType =
                    GetSelectedType(_inheritBaseTransitionWithContextEnumOption);
                Type[] inheritTypes = TypesHelpers.GetInheritGenericTypes(
                    _assemblyTypes,
                    selectedBaseTransitionWithContextType
                );

                inheritSelectedTransitionTypes.AddRange(inheritTypes);
            }

            CodeAnalyzer.Result codeAnalyzeResult = CodeAnalyzer.Analyze(
                inheritSelectedBaseStateTypes,
                inheritSelectedTransitionTypes.ToArray(),
                _sourceCodeDirectoryPath
            );

            HashSet<string> allSourceIds = new Dictionary<string, HashSet<string>>()
               .Concat(codeAnalyzeResult.fromStateToTransitionByTransition)
               .Concat(codeAnalyzeResult.fromTransitionToStateByState)
               .Concat(codeAnalyzeResult.fromOtherSourceToTransitionByTransition)
               .Concat(codeAnalyzeResult.fromOtherSourceToStateByState)
               .SelectMany(kv => new HashSet<string>(kv.Value.Union(new[] { kv.Key })))
               .ToHashSet();

            string stateGraphName = StringsHelpers
               .GetCommonSubstring(allSourceIds)
               .TrimEnd('.');

            string stateGraphDescription = $"FeatureName: {stateGraphName}";

            _sateGraphText = Graphviz.StateGraphGenerator.GenerateStateGraph(codeAnalyzeResult, stateGraphDescription);
            _sateGraphWithTransitionsText =
                Graphviz.StateGraphWithTransitionsGenerator.Generate(codeAnalyzeResult, stateGraphDescription);
        }
    }
}