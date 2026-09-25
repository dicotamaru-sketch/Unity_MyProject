using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class StandaloneBuilder
{
    private static readonly string[] Scenes = { "Assets/Scenes/FlowScene.unity" };

    [MenuItem("Flow/Build macOS")]
    public static void BuildMac()
    {
        Build(BuildTarget.StandaloneOSX, "Builds/macOS/FlowStudy.app");
    }

    [MenuItem("Flow/Build Windows")]
    public static void BuildWindows()
    {
        Build(BuildTarget.StandaloneWindows64, "Builds/Windows/FlowStudy.exe");
    }

    private static void Build(BuildTarget target, string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = Scenes,
            locationPathName = path,
            target = target,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Standalone build failed ({target}): {report.summary.result}");
        }
    }
}
