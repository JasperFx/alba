using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;


[GitHubActions(
    "Build & Test",
    GitHubActionsImage.UbuntuLatest,
    OnPushBranches = ["master"],
    OnPullRequestBranches = ["master"],
    InvokedTargets = [nameof(Test)],
    AutoGenerate = false)]
[GitHubActions(
    "Manual Nuget Push",
    GitHubActionsImage.UbuntuLatest,
    On = [GitHubActionsTrigger.WorkflowDispatch],
    InvokedTargets = [nameof(NugetPush)],
    ImportSecrets = [nameof(NugetApiKey)],
    AutoGenerate = false)]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    
    public static int Main () => Execute<Build>(x => x.Compile);
    
    [Solution(GenerateProjects = true)] readonly Solution Solution;
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration("Release")
                .EnableNoRestore());
        });
    
    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(_ => _
                .SetProjectFile(Solution.Alba_Testing));
            
            DotNetTest(_ => _
                .SetProjectFile(Solution.NUnitSamples));
        });

    Target NugetPack => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetPack(_ => _
                .SetProject(Solution.Alba)
                .SetConfiguration("Release")
                .EnableContinuousIntegrationBuild()
                .SetOutputDirectory(ArtifactsDirectory));
        });

    [Parameter("Nuget Api Key")] [Secret] readonly string NugetApiKey;

    Target NugetPush => _ => _
        .DependsOn(NugetPack)
        .Requires(() => !string.IsNullOrEmpty(NugetApiKey))
        .Executes(() =>
        {
            DotNetNuGetPush(_ => _
                .SetSource("https://api.nuget.org/v3/index.json")
                .SetTargetPath(ArtifactsDirectory / "*.nupkg")
                .EnableSkipDuplicate()
                .EnableNoSymbols()
                .SetApiKey(NugetApiKey));
        });
    
}
