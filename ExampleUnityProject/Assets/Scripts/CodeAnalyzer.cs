using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Frameworks.StateMachine.StateGraphVisualizer
{
    public static class CodeAnalyzer
    {
        public static void Analyze(Type[] inheritBaseStateTypes, Type[] inheritBaseTransitionTypes)
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
                    INamedTypeSymbol sourceNamedTypeSymbol = semanticModel.GetDeclaredSymbol(sourceClassDeclarationSyntax);
                    creationStateSources.Add(sourceNamedTypeSymbol);
                }
                else if (creationTransitionSourcesByTransition.TryGetValue(typeInfoTypeSymbol,
                    out HashSet<INamedTypeSymbol> creationTransitionSources))
                {
                    ClassDeclarationSyntax sourceClassDeclarationSyntax =
                        GetClassDeclarationSyntax(objectCreationExpressionSyntax);
                    INamedTypeSymbol sourceNamedTypeSymbol = semanticModel.GetDeclaredSymbol(sourceClassDeclarationSyntax);
                    creationTransitionSources.Add(sourceNamedTypeSymbol);
                }
            }

            UnityEngine.Debug.Log($"#{UnityEngine.Time.frameCount}:test ");
        }

        static ClassDeclarationSyntax GetClassDeclarationSyntax(SyntaxNode syntaxNode)
        {
            return syntaxNode.Parent as ClassDeclarationSyntax ?? GetClassDeclarationSyntax(syntaxNode.Parent);
        }
    }
}