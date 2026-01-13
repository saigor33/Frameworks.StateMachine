using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

        AnalyzeCode(inheritSelectedBaseStateTypes, inheritSelectedTransitionTypes);

        UnityEngine.Debug.Log($"#{UnityEngine.Time.frameCount}: Done");
    }

    static void AnalyzeCode(Type[] inheritBaseStateTypes, Type[] inheritBaseTransitionTypes)
    {
        string rootDirectoryPath =
            @"C:\MyFolder\Projects\Frameworks\StateMachine\ExampleUnityProject\Assets\Scripts\Match";

        string[] cSharpFilePaths = Directory.GetFiles(rootDirectoryPath, "*.cs", SearchOption.AllDirectories);
        string sourceCode = string.Join("\n", cSharpFilePaths.Select(File.ReadAllText));

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
        var compilation = CSharpCompilation.Create("MyCompilation", new[] { syntaxTree });
        SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
        SyntaxNode syntaxRoot = syntaxTree.GetRoot();

        Dictionary<ISymbol, HashSet<INamedTypeSymbol>> creationStateSourcesByState =
            inheritBaseStateTypes
               .Select(t => compilation.GetTypeByMetadataName(t.FullName))
               .ToDictionary(
                    symbol => symbol,
                    _ => new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default),
                    SymbolEqualityComparer.Default
                );

        Dictionary<ISymbol, HashSet<INamedTypeSymbol>> creationTransitionSourcesByTransition =
            inheritBaseTransitionTypes
               .Select(t => compilation.GetTypeByMetadataName(t.FullName))
               .ToDictionary(
                    symbol => symbol,
                    _ => new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default),
                    SymbolEqualityComparer.Default
                );

        IEnumerable<ObjectCreationExpressionSyntax> objectCreationExpressionSyntaxes = syntaxRoot
           .DescendantNodes()
           .OfType<ObjectCreationExpressionSyntax>();

        foreach (ObjectCreationExpressionSyntax objectCreationExpressionSyntax in objectCreationExpressionSyntaxes)
        {
            TypeInfo typeInfo = semanticModel.GetTypeInfo(objectCreationExpressionSyntax);
            ITypeSymbol typeInfoTypeSymbol = typeInfo.Type;

            if (typeInfoTypeSymbol == null)
            {
                continue;
            }

            if (creationStateSourcesByState.TryGetValue(typeInfoTypeSymbol,
                out HashSet<INamedTypeSymbol> creationStateSources))
            {
                ClassDeclarationSyntax sourceClassDeclarationSyntax =
                    GetClassDeclarationSyntax(objectCreationExpressionSyntax);
                INamedTypeSymbol namedTypeSymbol = semanticModel.GetDeclaredSymbol(sourceClassDeclarationSyntax);
                creationStateSources.Add(namedTypeSymbol);
            }
            else if (creationTransitionSourcesByTransition.TryGetValue(typeInfoTypeSymbol,
                out HashSet<INamedTypeSymbol> creationTransitionSources))
            {
                ClassDeclarationSyntax sourceClassDeclarationSyntax =
                    GetClassDeclarationSyntax(objectCreationExpressionSyntax);
                INamedTypeSymbol namedTypeSymbol = semanticModel.GetDeclaredSymbol(sourceClassDeclarationSyntax);
                creationTransitionSources.Add(namedTypeSymbol);
            }
        }

        UnityEngine.Debug.Log($"#{UnityEngine.Time.frameCount}:test ");
    }

    static ClassDeclarationSyntax GetClassDeclarationSyntax(SyntaxNode syntaxNode)
    {
        return syntaxNode.Parent as ClassDeclarationSyntax ?? GetClassDeclarationSyntax(syntaxNode.Parent);
    }
}