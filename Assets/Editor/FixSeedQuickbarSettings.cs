using UnityEngine;
using UnityEditor;

/// <summary>
/// Fixes SeedQuickbarUI serialized settings: maxSlots=9, firstKey=Alpha1.
/// Menu: Tools > Fix Seed Quickbar Settings
/// </summary>
public class FixSeedQuickbarSettings
{
    [MenuItem("Tools/Fix Seed Quickbar Settings")]
    public static void Fix()
    {
        var quickbar = Object.FindFirstObjectByType<SeedQuickbarUI>();
        if (quickbar == null)
        {
            Debug.LogError("[FixSeedQuickbar] SeedQuickbarUI not found in scene!");
            return;
        }

        quickbar.maxSlots = 9;
        quickbar.firstKey = KeyCode.Alpha1;
        quickbar.slotSize = new Vector2(80, 80);
        quickbar.maxBarWidth = 900;

        EditorUtility.SetDirty(quickbar);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(quickbar.gameObject.scene);

        Debug.Log("[FixSeedQuickbar] Settings fixed: maxSlots=9, firstKey=Alpha1, slotSize=80x80");
    }
}
