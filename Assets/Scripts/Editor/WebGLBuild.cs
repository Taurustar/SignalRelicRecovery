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

        [MenuItem("Signal Relic Recovery/Build WebGL for GitHub Pages")]
        public static void BuildForPages()
        {
            // GitHub Pages does not serve gzip Content-Encoding headers reliably,
            // so disable compression for the Pages build.
            var previousCompression = PlayerSettings.WebGL.compressionFormat;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

            string docsPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "docs");
            string buildPath = Path.Combine(docsPath, "WebGL");
            if (Directory.Exists(buildPath))
                Directory.Delete(buildPath, true);
            Directory.CreateDirectory(buildPath);

            var scenes = new[] { "Assets/Scenes/MainScene.unity" };
            var report = BuildPipeline.BuildPlayer(scenes, buildPath, BuildTarget.WebGL, BuildOptions.None);

            // Restore compression for local builds.
            PlayerSettings.WebGL.compressionFormat = previousCompression;

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
                Debug.Log($"WebGL build for Pages succeeded: {buildPath}");
            else
                Debug.LogError($"WebGL build for Pages failed: {report.summary.result}");
        }
    }
}
