using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Fix all UI elements with wrong scale and layout issues.
/// Run: Tools > Moonlit Garden > Fix All UI
/// </summary>
public class FixAllUI : EditorWindow
{
    [MenuItem("Tools/Moonlit Garden/Fix All UI")]
    public static void FixAll()
    {
        Debug.Log("=== FixAllUI: Starting ===");

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("FixAllUI: No Canvas found!");
            return;
        }

        // Fix all UI elements with wrong scale
        FixRectTransforms(canvas.transform);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== FixAllUI: Complete! ===");
    }

    private static void FixRectTransforms(Transform parent)
    {
        RectTransform[] allRects = parent.GetComponentsInChildren<RectTransform>(true);
        
        foreach (RectTransform rt in allRects)
        {
            // Skip Canvas itself
            if (rt.GetComponent<Canvas>() != null) continue;

            Vector3 currentScale = rt.localScale;
            
            // Fix scale if it's abnormally large (like 1.918 from old bugs)
            if (currentScale.x > 1.5f || currentScale.y > 1.5f || currentScale.z > 1.5f)
            {
                Vector3 oldScale = rt.localScale;
                rt.localScale = Vector3.one;
                Debug.Log($"FixAllUI: Fixed scale of '{rt.name}' from {oldScale} to (1,1,1)");
            }
        }

        // Fix specific HUD elements positioning
        FixTimeHUD(parent);
        FixStaminaHUD(parent);
        FixInventory(parent);
        FixGoldDisplay(parent);
    }

    private static void FixTimeHUD(Transform canvas)
    {
        Transform timeHud = FindChildRecursive(canvas, "TimeHUD");
        if (timeHud == null) return;

        RectTransform rt = timeHud.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // Top-left
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -10);
        rt.localScale = Vector3.one;
        
        Debug.Log("FixAllUI: Fixed TimeHUD position");
    }

    private static void FixStaminaHUD(Transform canvas)
    {
        Transform staminaHud = FindChildRecursive(canvas, "StaminaHUD");
        if (staminaHud == null) return;

        RectTransform rt = staminaHud.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); // Top-left below TimeHUD
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(10, -70);
        rt.localScale = Vector3.one;
        
        Debug.Log("FixAllUI: Fixed StaminaHUD position and scale");
    }

    private static void FixInventory(Transform canvas)
    {
        Transform inventory = FindChildRecursive(canvas, "Inventory");
        if (inventory == null) return;

        RectTransform rt = inventory.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0); // Bottom center
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.pivot = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, 10);
        rt.sizeDelta = new Vector2(600, 100);
        rt.localScale = Vector3.one;
        
        Debug.Log("FixAllUI: Fixed Inventory position and scale");
    }

    private static void FixGoldDisplay(Transform canvas)
    {
        // Find Gold HUD (might be named differently)
        Transform goldHud = FindChildRecursive(canvas, "GoldHUD") ?? FindChildRecursive(canvas, "GoldDisplay");
        if (goldHud == null) return;

        RectTransform rt = goldHud.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1); // Top-right
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-10, -10);
        rt.localScale = Vector3.one;
        
        Debug.Log("FixAllUI: Fixed Gold display position");
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        Transform result = parent.Find(name);
        if (result != null) return result;

        foreach (Transform child in parent)
        {
            result = FindChildRecursive(child, name);
            if (result != null) return result;
        }

        return null;
    }
}
