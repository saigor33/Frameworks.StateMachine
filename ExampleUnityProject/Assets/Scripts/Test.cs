using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityEngine;

public class Test : MonoBehaviour
{
    [ContextMenu("TestAnalysisCode")]
    public void TestAnalysisCode()
    {
        Type baseStateType = typeof(Frameworks.StateMachine.BaseState<>);
        Type baseTransitionType = typeof(Frameworks.StateMachine.BaseTransition<>);
        Type baseTransitionWithContextType = typeof(Frameworks.StateMachine.BaseTransition<,>);

        Type[] allAbstractClassTypes =
            AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(assembly => assembly.GetTypes())
               .Where(type => type.IsClass && type.IsAbstract).ToArray();

        Type[] inheritBaseStateTypes = allAbstractClassTypes
           .Where(abstractClassType => IsInheritType(abstractClassType, baseStateType))
           .ToArray();

        Type[] inheritTransitionTypes = allAbstractClassTypes
           .Where(abstractClassType => IsInheritType(abstractClassType, baseTransitionType) ||
                IsInheritType(abstractClassType, baseTransitionWithContextType))
           .ToArray();


        string rootDirectoryPath =
            @"C:\MyFolder\Projects\Frameworks\StateMachine\ExampleUnityProject\Assets\Scripts\Match";

        string[] cSharpFilePaths = Directory.GetFiles(rootDirectoryPath, "*.cs", SearchOption.AllDirectories);
        string sourceCode = string.Join("\n", cSharpFilePaths.Select(File.ReadAllText));

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("MyCompilation", new[] { syntaxTree });

        SyntaxNode syntaxRoot = syntaxTree.GetRoot();
        IEnumerable<SyntaxNode> syntaxNodes = syntaxRoot.DescendantNodes();

        UnityEngine.Debug.Log($"#{UnityEngine.Time.frameCount}: Done");
    }

    static bool IsInheritType(Type abstractClassType, Type baseStateType)
    {
        return abstractClassType.BaseType != null &&
            abstractClassType.BaseType.IsGenericType &&
            abstractClassType.BaseType.GetGenericTypeDefinition() == baseStateType;
    }
}