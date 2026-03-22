using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Creates a notification toast panel under Canvas and wires it to NotificationManager.
/// </summary>
public class SetupNotificationUI
{
    [MenuItem("MoonlitGarden/Setup Notification Toast UI")]
    public static void Setup()
    {
        // Find Canvas
        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("[SetupNotification] Canvas not found");
            return;
        }

        // Find NotificationManager
        var nmGO = GameObject.Find("NotificationManager");
        if (nmGO == null)
        {
            Debug.LogError("[SetupNotification] NotificationManager not found");
            return;
        }

        var nm = nmGO.GetComponent<NotificationManager>();
        if (nm == null)
        {
            Debug.LogError("[SetupNotification] NotificationManager component not found");
            return;
        }

        // Check if panel already exists
        var existing = canvas.transform.Find("NotificationPanel");
        if (existing != null)
        {
            Debug.Log("[SetupNotification] NotificationPanel already exists, re-wiring only");
            WireReferences(nm, existing.gameObject);
            return;
        }

        // Create NotificationPanel
        var panelGO = new GameObject("NotificationPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        panelGO.transform.SetParent(canvas.transform, false);

        var panelRect = panelGO.GetComponent<RectTransform>();
        // Position at top-center of screen
        panelRect.anchorMin = new Vector2(0.2f, 0.85f);
        panelRect.anchorMax = new Vector2(0.8f, 0.95f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Semi-transparent dark background
        var panelImage = panelGO.GetComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        // Try to use a rounded sprite if available
        var roundedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Rustic UI/UI-Singles/UI - 30.png");
        if (roundedSprite != null)
        {
            panelImage.sprite = roundedSprite;
            panelImage.type = Image.Type.Sliced;
        }

        // Start hidden
        var cg = panelGO.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        panelGO.SetActive(false);

        // Create Text child
        var textGO = new GameObject("NotificationText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(panelGO.transform, false);

        var textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(16, 4);
        textRect.offsetMax = new Vector2(-16, -4);

        var tmp = textGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "Notification";
        tmp.fontSize = 18;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableWordWrapping = true;

        // Make sure panel is on top
        panelGO.transform.SetAsLastSibling();

        // Wire references
        WireReferences(nm, panelGO);

        Debug.Log("[SetupNotification] Notification toast UI created and wired!");
    }

    private static void WireReferences(NotificationManager nm, GameObject panelGO)
    {
        nm.notificationPanel = panelGO;

        var textTMP = panelGO.GetComponentInChildren<TMP_Text>();
        if (textTMP != null)
        {
            nm.notificationText = textTMP;
            Debug.Log("[SetupNotification] Wired notificationText");
        }

        EditorUtility.SetDirty(nm);
        EditorUtility.SetDirty(panelGO);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
    }
}
