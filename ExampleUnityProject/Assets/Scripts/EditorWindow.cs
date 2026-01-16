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

        // [MenuItem("Tools/StateMachine/Visualization")]
        [MenuItem("Tools/StateMachineVisualization")]
        public static void ShowWindow()
        {
            GetWindow<EditorWindow>();
        }

        void Awake()
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

            OnGuiDrawSelectionState();

            // todo: add option "not all select transition"
            // select path
            // select transition
            // select typed transition
            _needVisualizeTransitions = EditorGUILayout.Toggle("Need visualize transitions", _needVisualizeTransitions);

            if (GUILayout.Button("Generate"))
            {
                GenerateGraphvizCode();
            }

            OnGuiDrawGenerationResult();

            GUILayout.EndVertical();
        }

        void OnGuiDrawGenerationResult()
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

        void OnGuiDrawSelectionState()
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

            _sateGraphText = sateGraphText;
            _sateGraphWithTransitionsText = sateGraphText;
        }
    }
}