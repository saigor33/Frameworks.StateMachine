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
        string _graphvizText;

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
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            GUILayout.Label("BaseState type");

            _inheritBaseStateEnumOption.selectedIndex = EditorGUILayout.Popup(
                _inheritBaseStateEnumOption.selectedIndex,
                _inheritBaseStateEnumOption.typeFullNames
            );

            GUILayout.EndHorizontal();

            // todo: add option "not all select transition"
            // select state
            // select transition
            // select typed transition
            _needVisualizeTransitions = EditorGUILayout.Toggle("Need visualize transitions", _needVisualizeTransitions);


            if (GUILayout.Button("Generate"))
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

                _graphvizText = string.Join("\n",
                    codeAnalyzeResult.fromStateToTransitionByTransition.Select(kv =>
                    {
                        (string transitionId, HashSet<string> stateIds) = kv;
                        return string.Join("\n", stateIds.Select(stateId => $"{stateId} -> {transitionId}"));
                    }));
            }

            _graphvizText = GUILayout.TextArea(_graphvizText);


            // text field with transition
            // text field without transition

            GUILayout.EndVertical();
        }
    }
}