using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Editor tool: assigns PixelLab-generated icons into the Settings panel UI.
/// Run via: Tools > Vietnamese Farmer > Assign Settings Icons
/// </summary>
public class AssignSettingsIcons
{
    [MenuItem("Tools/Vietnamese Farmer/Assign Settings Icons")]
    public static void Run()
    {
        Debug.Log("=== AssignSettingsIcons: Starting ===");

        // Load sprites
        Sprite bgmSprite    = LoadSprite("Assets/Sprites/UI/icon_bgm.png");
        Sprite sfxSprite    = LoadSprite("Assets/Sprites/UI/icon_sfx.png");
        Sprite screenSprite = LoadSprite("Assets/Sprites/UI/icon_screen.png");
        Sprite lotusSprite  = LoadSprite("Assets/Sprites/UI/icon_lotus.png");

        // Find icon GOs
        SetIcon("IconBGM",    bgmSprite);
        SetIcon("IconSFX",    sfxSprite);
        SetIcon("IconScreen", screenSprite);
        SetIcon("TitleLotus", lotusSprite);

        // Position icons precisely using RectTransform
        PositionIcon("IconBGM",    new Vector2(0f, 0.66f), new Vector2(0.1f, 0.78f), new Vector2(4, 2), new Vector2(-4, -2));
        PositionIcon("IconSFX",    new Vector2(0f, 0.52f), new Vector2(0.1f, 0.64f), new Vector2(4, 2), new Vector2(-4, -2));
        PositionIcon("IconScreen", new Vector2(0f, 0.36f), new Vector2(0.1f, 0.50f), new Vector2(4, 2), new Vector2(-4, -2));
        // Lotus: top-center title decoration
        PositionIcon("TitleLotus", new Vector2(0.43f, 0.82f), new Vector2(0.57f, 1f), new Vector2(-2, 2), new Vector2(2, -2));

        // Set preserve aspect on all icons
        foreach (string name in new[] { "IconBGM", "IconSFX", "IconScreen", "TitleLotus" })
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                var img = go.GetComponent<Image>();
                if (img != null) img.preserveAspect = true;
            }
        }

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("=== AssignSettingsIcons: Done! ===");
        EditorUtility.DisplayDialog("Done", "Settings icons assigned successfully!", "OK");
    }

    private static Sprite LoadSprite(string path)
    {
        // Force reimport to ensure metadata is applied
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            Debug.LogWarning($"AssignSettingsIcons: Could not load sprite at '{path}'");
        else
            Debug.Log($"AssignSettingsIcons: Loaded sprite '{sprite.name}'");
        return sprite;
    }

    private static void SetIcon(string goName, Sprite sprite)
    {
        if (sprite == null) return;
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"AssignSettingsIcons: '{goName}' not found in scene."); return; }
        var img = go.GetComponent<Image>();
        if (img == null) { Debug.LogWarning($"AssignSettingsIcons: '{goName}' has no Image component."); return; }
        img.sprite = sprite;
        img.color  = Color.white;
        img.raycastTarget = false;
        Debug.Log($"AssignSettingsIcons: Assigned sprite to '{goName}'");
    }

    private static void PositionIcon(string goName, Vector2 anchorMin, Vector2 anchorMax,
                                     Vector2 offsetMin, Vector2 offsetMax)
    {
        var go = GameObject.Find(goName);
        if (go == null) return;
        var rt = go.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.offsetMin  = offsetMin;
        rt.offsetMax  = offsetMax;
    }
}
