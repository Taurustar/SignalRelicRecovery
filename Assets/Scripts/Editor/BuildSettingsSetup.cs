using UnityEditor;
using UnityEngine;

namespace SignalRelicRecovery.Editor
{
    public static class BuildSettingsSetup
    {
        [MenuItem("Signal Relic Recovery/Configure WebGL Build")]
        public static void Run()
        {
            string scenePath = "Assets/Scenes/MainScene.unity";
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
            {
                Debug.LogError("MainScene not found at " + scenePath);
                return;
            }

            var scenes = new[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = scenes;

            // Switch to WebGL if not already.
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            }

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.template = "APPLICATION:Default";
            PlayerSettings.productName = "Signal Relic Recovery";

            Debug.Log("Signal Relic Recovery: WebGL build settings configured.");
        }
    }
}
