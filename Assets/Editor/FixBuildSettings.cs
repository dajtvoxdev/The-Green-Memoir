using UnityEditor;

/// <summary>
/// Cập nhật Build Settings: thay LoadingScene (đã xóa) bằng AsyncLoadingScene.
/// Chạy 1 lần rồi xóa.
/// </summary>
public class FixBuildSettings
{
    [MenuItem("Tools/FixBuildSettings")]
    public static void Fix()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/LoginScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/AsyncLoadingScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/PlayScene.unity", true)
        };
        EditorBuildSettings.scenes = scenes;
        UnityEngine.Debug.Log("Build Settings da cap nhat: LoginScene(0), AsyncLoadingScene(1), PlayScene(2)");
    }
}
