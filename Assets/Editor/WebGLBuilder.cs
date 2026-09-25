using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class WebGLBuilder
{
    private const string BuildPath = "docs";

    public static void BuildGitHubPages()
    {
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

        if (Directory.Exists(BuildPath))
        {
            Directory.Delete(BuildPath, true);
        }

        Directory.CreateDirectory(BuildPath);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/FlowScene.unity" },
            locationPathName = BuildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        File.WriteAllText(Path.Combine(BuildPath, ".nojekyll"), string.Empty);

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"WebGL build failed: {report.summary.result}");
        }
    }
}
