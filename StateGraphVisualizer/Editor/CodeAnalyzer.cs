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
        public class Result
        {
            public HashSet<string> states;
            public HashSet<string> transitions;
            public Dictionary<string, HashSet<string>> fromTransitionToStateByState;
            public Dictionary<string, HashSet<string>> fromOtherSourceToStateByState;
            public Dictionary<string, HashSet<string>> fromStateToTransitionByTransition;
            public Dictionary<string, HashSet<string>> fromOtherSourceToTransitionByTransition;
        }

        public static Result Analyze(Type[] inheritBaseStateTypes, Type[] inheritBaseTransitionTypes,
            string sourceCodePath)
        {
            string[] cSharpFilePaths = Directory.GetFiles(sourceCodePath, "*.cs", SearchOption.AllDirectories);
            string sourceCode = string.Join("\n", cSharpFilePaths.Select(File.ReadAllText));

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            var compilation = CSharpCompilation.Create("MyCompilation", new[] { syntaxTree });
            SemanticModel semanticModel = compilation.GetSemanticModel(syntaxTree);
            SyntaxNode syntaxRoot = syntaxTree.GetRoot();

            ObjectCreationExpressionSyntax[] objectCreationExpressionSyntaxes = syntaxRoot
               .DescendantNodes()
               .OfType<ObjectCreationExpressionSyntax>()
               .ToArray();

            HashSet<ISymbol> stateSymbols = inheritBaseStateTypes
               .Select(t => compilation.GetTypeByMetadataName(t.FullName))
               .ToHashSet(SymbolEqualityComparer.Default);

            HashSet<ISymbol> transitionSymbols = inheritBaseTransitionTypes
               .Select(t => compilation.GetTypeByMetadataName(t.FullName))
               .ToHashSet(SymbolEqualityComparer.Default);

            Dictionary<ISymbol, HashSet<INamedTypeSymbol>> creationStateSourcesByState =
                GetCreationSymbolSourcesBySymbol(semanticModel, objectCreationExpressionSyntaxes, stateSymbols);
            Dictionary<ISymbol, HashSet<INamedTypeSymbol>> creationTransitionSourcesByTransition =
                GetCreationSymbolSourcesBySymbol(semanticModel, objectCreationExpressionSyntaxes, transitionSymbols);

            Dictionary<ISymbol, HashSet<ISymbol>> fromTransitionToStateByState = FilterValuesByPredicate(
                targetSymbols: stateSymbols,
                allSymbolSourcesByTargetSymbol: creationStateSourcesByState,
                allSymbolSourcesFilter: symbol => transitionSymbols.Contains(symbol)
            );

            Dictionary<ISymbol, HashSet<ISymbol>> fromOtherSourceToStateByState = FilterValuesByPredicate(
                targetSymbols: stateSymbols,
                allSymbolSourcesByTargetSymbol: creationStateSourcesByState,
                allSymbolSourcesFilter: symbol => !transitionSymbols.Contains(symbol)
            );

            Dictionary<ISymbol, HashSet<ISymbol>> fromStateToTransitionByTransition = FilterValuesByPredicate(
                targetSymbols: transitionSymbols,
                allSymbolSourcesByTargetSymbol: creationTransitionSourcesByTransition,
                allSymbolSourcesFilter: symbol => stateSymbols.Contains(symbol)
            );

            Dictionary<ISymbol, HashSet<ISymbol>> fromOtherSourceToTransitionByTransition = FilterValuesByPredicate(
                targetSymbols: transitionSymbols,
                allSymbolSourcesByTargetSymbol: creationTransitionSourcesByTransition,
                allSymbolSourcesFilter: symbol => !stateSymbols.Contains(symbol)
            );

            return new Result
            {
                states = ConvertSymbolsToNames(stateSymbols),
                transitions = ConvertSymbolsToNames(transitionSymbols),
                fromTransitionToStateByState = ConvertSymbolsToNames(fromTransitionToStateByState),
                fromOtherSourceToStateByState = ConvertSymbolsToNames(fromOtherSourceToStateByState),
                fromStateToTransitionByTransition = ConvertSymbolsToNames(fromStateToTransitionByTransition),
                fromOtherSourceToTransitionByTransition = ConvertSymbolsToNames(fromOtherSourceToTransitionByTransition)
            };
        }

        static Dictionary<ISymbol, HashSet<ISymbol>> FilterValuesByPredicate(HashSet<ISymbol> targetSymbols,
            Dictionary<ISymbol, HashSet<INamedTypeSymbol>> allSymbolSourcesByTargetSymbol,
            Func<ISymbol, bool> allSymbolSourcesFilter)
        {
            return targetSymbols
               .ToDictionary(
                    targetSymbol => targetSymbol,
                    stateSymbol => allSymbolSourcesByTargetSymbol[stateSymbol]
                       .Where(s => allSymbolSourcesFilter(s))
                       .ToHashSet(SymbolEqualityComparer.Default),
                    SymbolEqualityComparer.Default
                );
        }

        static Dictionary<string, HashSet<string>> ConvertSymbolsToNames(Dictionary<ISymbol, HashSet<ISymbol>> source)
        {
            return source.ToDictionary(
                kv => GetSymbolName(kv.Key),
                kv => ConvertSymbolsToNames(kv.Value)
            );
        }

        static HashSet<string> ConvertSymbolsToNames(HashSet<ISymbol> symbols)
        {
            return symbols
               .Select(GetSymbolName)
               .ToHashSet();
        }

        static string GetSymbolName(ISymbol symbol)
        {
            return symbol.ToDisplayString();
        }

        static Dictionary<ISymbol, HashSet<INamedTypeSymbol>> GetCreationSymbolSourcesBySymbol(
            SemanticModel semanticModel, ObjectCreationExpressionSyntax[] objectCreationExpressionSyntaxes,
            HashSet<ISymbol> creationSymbols)
        {
            Dictionary<ISymbol, HashSet<INamedTypeSymbol>> result = creationSymbols
               .ToDictionary(
                    symbol => symbol,
                    _ => new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default),
                    SymbolEqualityComparer.Default
                );

            foreach (ObjectCreationExpressionSyntax objectCreationExpressionSyntax in objectCreationExpressionSyntaxes)
            {
                TypeInfo typeInfo = semanticModel.GetTypeInfo(objectCreationExpressionSyntax);
                ITypeSymbol typeInfoTypeSymbol = typeInfo.Type;

                if (typeInfoTypeSymbol == null)
                {
                    continue;
                }

                if (result.TryGetValue(typeInfoTypeSymbol, out HashSet<INamedTypeSymbol> creationStateSources))
                {
                    ClassDeclarationSyntax sourceClassDeclarationSyntax =
                        GetClassDeclarationSyntax(objectCreationExpressionSyntax);
                    INamedTypeSymbol sourceNamedTypeSymbol =
                        semanticModel.GetDeclaredSymbol(sourceClassDeclarationSyntax);
                    creationStateSources.Add(sourceNamedTypeSymbol);
                }
            }

            return result;
        }

        static ClassDeclarationSyntax GetClassDeclarationSyntax(SyntaxNode syntaxNode)
        {
            return syntaxNode.Parent as ClassDeclarationSyntax ?? GetClassDeclarationSyntax(syntaxNode.Parent);
        }
    }
}