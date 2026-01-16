using System;
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
            public string[] values;
        }

        bool _needVisualizeTransitions;
        string[] _inheritBaseStateTypeFullNames;
        EnumOption _inheritBaseStateEnumOption;

        // [MenuItem("Tools/StateMachine/Visualization")]
        [MenuItem("Tools/StateMachineVisualization")]
        public static void ShowWindow()
        {
            GetWindow<EditorWindow>();
        }

        void Awake()
        {
            Type baseStateType = typeof(BaseState<>);
            Type baseTransitionType = typeof(BaseTransition<>);
            Type baseTransitionWithContextType = typeof(BaseTransition<,>);

            Type[] assemblyTypes = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(assembly => assembly.GetTypes())
               .ToArray();
            Type[] allAbstractClassTypes = assemblyTypes
               .Where(type => type.IsClass && type.IsAbstract)
               .ToArray();

            _inheritBaseStateEnumOption = new EnumOption
            {
                selectedIndex = 0, // todo: array can be empty
                values = TypesHelpers.GetInheritGenericTypes(allAbstractClassTypes, baseStateType)
                   .Select(type => type.FullName)
                   .ToArray()
            };
        }

        void OnGUI()
        {
            _inheritBaseStateEnumOption.selectedIndex =
                EditorGUILayout.Popup(_inheritBaseStateEnumOption.selectedIndex, _inheritBaseStateEnumOption.values);

            // todo: add option "not all select transition"
            // select state
            // select transition
            // select typed transition
            _needVisualizeTransitions = EditorGUILayout.Toggle("Need visualize transitions", _needVisualizeTransitions);


            if (GUILayout.Button("Generate"))
            {
                // generate text
            }


            // text field with transition
            // text field without transition
        }
    }
}