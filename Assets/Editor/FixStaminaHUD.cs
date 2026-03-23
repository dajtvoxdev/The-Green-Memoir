using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Fixes the StaminaHUD: adds missing StaminaText, wires all references.
/// Run: Tools > Moonlit Garden > Fix Stamina HUD
/// </summary>
public class FixStaminaHUD : EditorWindow
{
    [MenuItem("Tools/Moonlit Garden/Fix Stamina HUD")]
    public static void Fix()
    {
        Debug.Log("=== FixStaminaHUD: Starting ===");

        StaminaHUD hud = Object.FindFirstObjectByType<StaminaHUD>(FindObjectsInactive.Include);
        if (hud == null)
        {
            Debug.LogError("FixStaminaHUD: No StaminaHUD found in scene!");
            return;
        }

        // Ensure HUD is active
        if (!hud.gameObject.activeSelf)
        {
            hud.gameObject.SetActive(true);
            Debug.Log("FixStaminaHUD: Activated StaminaHUD GameObject");
        }

        // Wire staminaSlider
        if (hud.staminaSlider == null)
        {
            Slider slider = hud.GetComponentInChildren<Slider>(true);
            if (slider != null)
            {
                Undo.RecordObject(hud, "Wire StaminaHUD slider");
                hud.staminaSlider = slider;
                Debug.Log("FixStaminaHUD: Wired staminaSlider");
            }
            else
            {
                Debug.LogWarning("FixStaminaHUD: No Slider child found!");
            }
        }

        // Wire fillImage from slider's fill rect
        if (hud.fillImage == null && hud.staminaSlider != null && hud.staminaSlider.fillRect != null)
        {
            Image fill = hud.staminaSlider.fillRect.GetComponent<Image>();
            if (fill != null)
            {
                Undo.RecordObject(hud, "Wire StaminaHUD fillImage");
                hud.fillImage = fill;
                Debug.Log("FixStaminaHUD: Wired fillImage");
            }
        }

        // Set fill color to green (healthy stamina)
        if (hud.fillImage != null)
        {
            Undo.RecordObject(hud.fillImage, "Set fill color");
            hud.fillImage.color = new Color(0.28f, 0.82f, 0.28f, 1f);
        }

        // Create StaminaText if missing
        Transform textT = hud.transform.Find("StaminaText");
        if (textT == null)
        {
            GameObject textObj = new GameObject("StaminaText", typeof(RectTransform));
            textObj.transform.SetParent(hud.transform, false);
            Undo.RegisterCreatedObjectUndo(textObj, "Create StaminaText");

            RectTransform rt = textObj.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(5, 0);
            rt.offsetMax = new Vector2(-5, 0);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "50/50";
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color32(255, 245, 220, 255);
            tmp.raycastTarget = false;

            Undo.RecordObject(hud, "Wire StaminaHUD text");
            hud.staminaText = tmp;
            Debug.Log("FixStaminaHUD: Created and wired StaminaText");
        }
        else
        {
            TMP_Text existingText = textT.GetComponent<TMP_Text>();
            if (existingText != null && hud.staminaText == null)
            {
                Undo.RecordObject(hud, "Wire StaminaHUD text");
                hud.staminaText = existingText;
                Debug.Log("FixStaminaHUD: Wired existing StaminaText");
            }
        }

        // Remove old legacy Text label if exists
        Transform oldLabel = hud.transform.Find("Label");
        if (oldLabel != null)
        {
            var legacyText = oldLabel.GetComponent<UnityEngine.UI.Text>();
            if (legacyText != null)
            {
                Undo.DestroyObjectImmediate(oldLabel.gameObject);
                Debug.Log("FixStaminaHUD: Removed old legacy Text label");
            }
        }

        // Make slider non-interactable (display only)
        if (hud.staminaSlider != null)
        {
            Undo.RecordObject(hud.staminaSlider, "Set slider non-interactable");
            hud.staminaSlider.interactable = false;
        }

        EditorUtility.SetDirty(hud);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== FixStaminaHUD: Complete! ===");
    }
}
