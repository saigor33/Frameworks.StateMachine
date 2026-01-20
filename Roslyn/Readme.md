Roslyn - Microsoft code analysis and code generation tools.
url: https://github.com/dotnet/roslyn

NuGetForUnity: https://openupm.com/packages/com.github-glitchenzo.nugetforunity/

Installation:
- Disable package from unity project
- Use NuGetForUnity (automatic resolve dependencies) install "Microsoft.CodeAnalysis.CSharp" in the project.
    P.S. Need check version Unity supported. For Unity 6000.3.3f1 it is Microsoft.CodeAnalysis.CSharp ver. 4.3.
    For more information check: https://docs.unity3d.com/6000.3/Documentation/Manual/create-source-generator.html.
- Remove all files from /Editor/ folder
- Copy new version Microsoft.CodeAnalysis.CSharp and dependencies from NuGetForUnity install packages location folder
    P.S. See NuGet -> Preferences window, "Placament" property.
    By default path: <UnityProject>/Packages/nuget-packages/InstalledPackages/ or <UnityProject>/Assets/Packages/
    
    
Roslyn exported as local package because NuGetForUnity restore packages on opened Unity project after Unity finish compilation code.
It is create code compilation errors. 
This errors can be resolved if needed (See NuGetForUnity instruction: https://openupm.com/packages/com.github-glitchenzo.nugetforunity/)
