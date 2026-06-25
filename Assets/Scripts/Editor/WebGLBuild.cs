using UnityEditor;
using UnityEngine;
using System.IO;

namespace SignalRelicRecovery.Editor
{
    public static class WebGLBuild
    {
        [MenuItem("Signal Relic Recovery/Build WebGL")]
        public static void Build()
        {
            string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Builds", "WebGL");
            if (!Directory.Exists(buildPath))
                Directory.CreateDirectory(buildPath);

            var scenes = new[] { "Assets/Scenes/MainScene.unity" };
            var report = BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WebGL, BuildOptions.None);

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                Debug.Log($"WebGL build succeeded: {buildPath}");
            else
                Debug.LogError($"WebGL build failed: {report.summary.result}");
        }
    }
}
