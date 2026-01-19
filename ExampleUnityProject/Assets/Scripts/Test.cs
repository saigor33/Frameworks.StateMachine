using System;
using System.Linq;
using Frameworks.StateMachine.StateGraphVisualizer;
using UnityEngine;

public class Test : MonoBehaviour
{
    [ContextMenu("TestAnalysisCode")]
    public void TestAnalysisCode()
    {
        Type baseStateType = typeof(Frameworks.StateMachine.BaseState<>);
        Type baseTransitionType = typeof(Frameworks.StateMachine.BaseTransition<>);
        Type baseTransitionWithContextType = typeof(Frameworks.StateMachine.BaseTransition<,>);

        Type[] assemblyTypes =
            AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(assembly => assembly.GetTypes()).ToArray();
        Type[] allAbstractClassTypes = assemblyTypes.Where(type => type.IsClass && type.IsAbstract).ToArray();

        Type[] inheritBaseStateTypes = TypesHelpers.GetInheritGenericTypes(allAbstractClassTypes, baseStateType);
        Type[] inheritTransitionTypes = new[]
            {
                TypesHelpers.GetInheritGenericTypes(assemblyTypes, baseTransitionType),
                TypesHelpers.GetInheritGenericTypes(assemblyTypes, baseTransitionWithContextType),
            }
           .SelectMany(t => t)
           .ToArray();

        Type selectedBaseStateType = typeof(Match.Logic.BaseState);
        Type selectedTransitionType = typeof(Match.Logic.BaseTransition);
        Type selectedTransitionWitchContextType = typeof(Match.Logic.BaseTransition<>);

        Type[] inheritSelectedBaseStateTypes = TypesHelpers.GetInheritTypes(assemblyTypes, selectedBaseStateType);

        Type[] inheritSelectedTransitionTypes = new[]
            {
                TypesHelpers.GetInheritTypes(assemblyTypes, selectedTransitionType),
                TypesHelpers.GetInheritGenericTypes(assemblyTypes, selectedTransitionWitchContextType)
            }
           .SelectMany(t => t)
           .ToArray();

        CodeAnalyzer.Result result = CodeAnalyzer.Analyze(inheritSelectedBaseStateTypes, inheritSelectedTransitionTypes, @"C:\MyFolder\Projects\Frameworks\StateMachine\ExampleUnityProject\Assets\Scripts\Match");

        UnityEngine.Debug.Log($"#{UnityEngine.Time.frameCount}: Done");
    }
}