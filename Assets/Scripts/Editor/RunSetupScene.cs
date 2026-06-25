using UnityEditor;
using SignalRelicRecovery.Editor;

public static class RunSetupScene
{
    [MenuItem("Signal Relic Recovery/Run Setup Scene Now")]
    public static void Execute()
    {
        SetupScene.Run();
    }
}
