using System;
using System.IO;
using System.Text.RegularExpressions;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    internal class Build
    {
        private const string BUILD_VERSION = "6.1.0";
        private const string GITHUB_REPO = "https://github.com/jasperfx/alba.git";


        private static void Main(string[] args)
        {
            
            Target("default", DependsOn("test"));
            
            Target("restore", () =>
            {
                Run("dotnet", "restore src/Alba.sln");
            });
           
            Target("test", DependsOn("restore"),() =>
            {
                RunTests("Alba.Testing");
                RunTests("NUnitSamples");
            });

            Target("ci", DependsOn("default"));

            Target("install", () =>
                RunNpm("install"));

            Target("compile",  DependsOn("restore"),() =>
            {
                Run("dotnet",
                    $"build src/Alba.sln");
            });
            
            Target("install-mdsnippets", IgnoreIfFailed(() =>
                Run("dotnet", $"tool install -g MarkdownSnippets.Tool")
            ));

            Target("docs", DependsOn("install", "install-mdsnippets"), () => {
                // Run docs site
                RunNpm("run docs");
            });

            Target("docs-build", DependsOn("install", "install-mdsnippets"), () => {
                // Run docs site
                RunNpm("run docs-build");
            });

            Target("clear-inline-samples", () => {
                var files = Directory.GetFiles("./docs", "*.md", SearchOption.AllDirectories);
                var pattern = @"<!-- snippet:(.+)-->[\s\S]*?<!-- endSnippet -->";
                var replacePattern = $"<!-- snippet:$1-->{Environment.NewLine}<!-- endSnippet -->";
                foreach (var file in files)
                {
                    // Console.WriteLine(file);
                    var content = File.ReadAllText(file);

                    if (!content.Contains("<!-- snippet:")) {
                        continue;
                    }

                    var updatedContent = Regex.Replace(content, pattern, replacePattern);
                    File.WriteAllText(file, updatedContent);
                }
            });

            Target("publish-docs", DependsOn("docs-build"), () =>
            {
                PublishDocs(GITHUB_REPO);
            });

            Target("publish-docs-gh-actions", DependsOn("docs-build"), () =>
            {
                var ghActor = GetEnvironmentVariable("GITHUB_ACTOR");
                var ghToken = GetEnvironmentVariable("GITHUB_TOKEN");
                var ghRepo = GetEnvironmentVariable("GITHUB_REPOSITORY");
                var repo = $"https://{ghActor}:{ghToken}@github.com/{ghRepo}.git";
                PublishDocs(repo, true);
            });

            RunTargetsAndExit(args);
        }

        private static void PublishDocs(string repo, bool isGHActionContext=false)
        {
            var docTargetDir = "doc-target";
            var branchName = "gh-pages";
            Run("git", $"clone -b {branchName} {repo} {InitializeDirectory(docTargetDir)}");

            if (isGHActionContext) {
                Run("git", "config user.email \"action@github.com\"", docTargetDir);
                Run("git", "config user.name \"GitHub Action\"", docTargetDir);
            } else {
                // if you are not using git --global config, un-comment the block below, update and use it
                // Run("git", "config user.email user_email", docTargetDir);
                // Run("git", "config user.name user_name", docTargetDir);
            }

            // Clean off all the files in the doc-target directory
            foreach (var file in Directory.EnumerateFiles(docTargetDir))
            {
                if (Path.GetFileName(file) == ".nojekyll") continue;
                
                File.Delete(file);
            }

            var buildDir = Path.Combine(Environment.CurrentDirectory, "docs", ".vitepress", "dist");

            CopyFilesRecursively(buildDir, docTargetDir);

            Run("git", "add --all", docTargetDir);
            Run("git", $"commit -a -m \"Documentation Update for {BUILD_VERSION}\" --allow-empty", docTargetDir);
            Run("git", $"push origin {branchName}", docTargetDir);
        }


        private static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }

        private static string InitializeDirectory(string path)
        {
            EnsureDirectoriesDeleted(path);
            Directory.CreateDirectory(path);
            return path;
        }

        private static void EnsureDirectoriesDeleted(params string[] paths)
        {
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    DeleteDirectory(dir);
                }
            }
        }

        private static void DeleteDirectory(DirectoryInfo baseDir)
        {
            baseDir.Attributes = FileAttributes.Normal;
            foreach (var childDir in baseDir.GetDirectories())
                DeleteDirectory(childDir);

            foreach (var file in baseDir.GetFiles())
                file.IsReadOnly = false;

            baseDir.Delete(true);
        }

        private static void RunNpm(string args) =>
            Run("npm", args, windowsName: "cmd.exe", windowsArgs: $"/c npm {args}");

        private static void RunTests(string projectName, string directoryName = "src")
        {
            Run("dotnet", $"test --no-restore {directoryName}/{projectName}/{projectName}.csproj");
        }
        
        private static string GetEnvironmentVariable(string variableName)
        {
            var val = Environment.GetEnvironmentVariable(variableName);

            // Azure devops converts environment variable to upper case and dot to underscore
            // https://docs.microsoft.com/en-us/azure/devops/pipelines/process/variables?view=azure-devops&tabs=yaml%2Cbatch
            // Attempt to fetch variable by updating it
            if (string.IsNullOrEmpty(val))
            {
                val = Environment.GetEnvironmentVariable(variableName.ToUpper().Replace(".", "_"));
            }

            Console.WriteLine(val);

            return val;
        }

        private static Action IgnoreIfFailed(Action action)
        {
            return () =>
            {
                try
                {
                    action();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
                }
            };
        }
    }
}
