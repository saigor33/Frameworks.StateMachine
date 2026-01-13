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
}