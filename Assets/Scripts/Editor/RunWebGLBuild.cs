using UnityEditor;
using SignalRelicRecovery.Editor;

public static class RunWebGLBuild
{
    [MenuItem("Signal Relic Recovery/Run WebGL Build for Pages")]
    public static void Execute()
    {
        WebGLBuild.BuildForPages();
    }

    [MenuItem("Signal Relic Recovery/Run WebGL Accessibility Build for Pages")]
    public static void ExecuteAccessibility()
    {
        WebGLBuild.BuildForPagesAccessibility();
    }
}
