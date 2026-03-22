using UnityEngine;
using UnityEditor;

/// <summary>
/// Wires all seed icon sprites to the SeedQuickbarUI.seedIcons serialized array.
/// Menu: Tools > Wire Seed Quickbar Icons
/// </summary>
public class WireSeedQuickbarIcons
{
    [MenuItem("Tools/Wire Seed Quickbar Icons")]
    public static void Wire()
    {
        var quickbar = Object.FindFirstObjectByType<SeedQuickbarUI>();
        if (quickbar == null)
        {
            Debug.LogError("[WireSeedIcons] SeedQuickbarUI not found in scene!");
            return;
        }

        string[] cropIds = {
            "bean", "corn", "wheat", "carrot", "potato", "onion",
            "cucumber", "tomato", "cabbage", "chili", "eggplant",
            "garlic", "pumpkin", "strawberry", "watermelon",
            "dragon_fruit", "ginseng", "rose"
        };

        var serializedObj = new SerializedObject(quickbar);
        var seedIconsProp = serializedObj.FindProperty("seedIcons");
        seedIconsProp.arraySize = cropIds.Length;

        int assigned = 0;
        for (int i = 0; i < cropIds.Length; i++)
        {
            string cropId = cropIds[i];
            string iconName = "Seed_" + cropId.Substring(0, 1).ToUpper() + cropId.Substring(1);
            string iconPath = "Assets/UI/" + iconName + ".png";

            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

            var element = seedIconsProp.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("itemIdPrefix").stringValue = cropId;
            element.FindPropertyRelative("icon").objectReferenceValue = sprite;

            if (sprite != null)
            {
                assigned++;
                Debug.Log("[WireSeedIcons] Assigned: " + cropId + " -> " + iconPath);
            }
            else
            {
                Debug.LogWarning("[WireSeedIcons] Sprite not found: " + iconPath);
            }
        }

        serializedObj.ApplyModifiedProperties();
        EditorUtility.SetDirty(quickbar);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(quickbar.gameObject.scene);

        Debug.Log("[WireSeedIcons] Wired " + assigned + "/" + cropIds.Length + " seed icons to SeedQuickbarUI.");
    }
}
