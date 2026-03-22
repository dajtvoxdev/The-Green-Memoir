using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class CleanupShopPanel
{
    [MenuItem("MoonlitGarden/Cleanup Shop Panel")]
    public static void Run()
    {
        // GameObject.Find only finds active objects — use transform search instead
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[CleanupShopPanel] No Canvas found!");
            return;
        }

        Transform panelTransform = null;
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            var child = canvas.transform.GetChild(i);
            if (child.name == "ShopPanel")
            {
                panelTransform = child;
                break;
            }
        }

        if (panelTransform == null)
        {
            Debug.LogError("[CleanupShopPanel] ShopPanel not found under Canvas!");
            return;
        }

        var panel = panelTransform.gameObject;

        // Temporarily activate to work with it
        bool wasActive = panel.activeSelf;
        panel.SetActive(true);

        // 1. Remove duplicate Content objects under Viewport
        var viewport = panelTransform.Find("ScrollArea/Viewport");
        if (viewport != null)
        {
            for (int i = viewport.childCount - 1; i >= 0; i--)
            {
                var child = viewport.GetChild(i);
                if (child.name == "Content" && child.GetComponent<VerticalLayoutGroup>() == null)
                {
                    Debug.Log($"[CleanupShopPanel] Deleting duplicate Content (index {i})");
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        // 2. Create Header if missing (only needs CloseButton)
        var header = panelTransform.Find("Header");
        if (header == null)
        {
            Debug.Log("[CleanupShopPanel] Creating Header with CloseButton...");

            var headerGO = new GameObject("Header");
            headerGO.transform.SetParent(panelTransform, false);
            // Move Header to be first child
            headerGO.transform.SetSiblingIndex(0);

            var headerRT = headerGO.AddComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0, 0.88f);
            headerRT.anchorMax = Vector2.one;
            headerRT.offsetMin = Vector2.zero;
            headerRT.offsetMax = Vector2.zero;

            // Header background
            headerGO.AddComponent<CanvasRenderer>();
            var headerImg = headerGO.AddComponent<Image>();

            // Try to use item_slot_bg sprite
            var headerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Shop/item_slot_bg_128.png");
            if (headerSprite != null)
            {
                headerImg.sprite = headerSprite;
                headerImg.color = Color.white;
            }
            else
            {
                headerImg.color = new Color(0.18f, 0.12f, 0.07f, 1f);
            }

            header = headerGO.transform;
        }

        // 3. Create CloseButton if missing inside Header
        var closeBtn = header.Find("CloseButton");
        if (closeBtn == null)
        {
            Debug.Log("[CleanupShopPanel] Creating CloseButton...");

            var closeBtnGO = new GameObject("CloseButton");
            closeBtnGO.transform.SetParent(header, false);

            var btnRT = closeBtnGO.AddComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(0.92f, 0.1f);
            btnRT.anchorMax = new Vector2(0.99f, 0.9f);
            btnRT.offsetMin = Vector2.zero;
            btnRT.offsetMax = Vector2.zero;

            closeBtnGO.AddComponent<CanvasRenderer>();
            var btnImg = closeBtnGO.AddComponent<Image>();

            // Try to use btn_close sprite
            var closeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Shop/btn_close_128.png");
            if (closeSprite != null)
            {
                btnImg.sprite = closeSprite;
                btnImg.color = Color.white;
            }
            else
            {
                btnImg.color = new Color(0.7f, 0.2f, 0.2f, 1f);
            }

            var btn = closeBtnGO.AddComponent<Button>();
            btn.targetGraphic = btnImg;

            // Add "X" text as child
            var textGO = new GameObject("CloseText");
            textGO.transform.SetParent(closeBtnGO.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "X";
            tmp.fontSize = 24;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            closeBtn = closeBtnGO.transform;
        }

        // 4. Remove ShopTitle and GoldDisplay if they exist (not needed)
        var shopTitle = header.Find("ShopTitle");
        if (shopTitle != null)
        {
            Object.DestroyImmediate(shopTitle.gameObject);
            Debug.Log("[CleanupShopPanel] Removed ShopTitle");
        }
        var goldDisplay = header.Find("GoldDisplay");
        if (goldDisplay != null)
        {
            Object.DestroyImmediate(goldDisplay.gameObject);
            Debug.Log("[CleanupShopPanel] Removed GoldDisplay");
        }

        // 5. Wire ShopPanel references
        var shopPanel = panel.GetComponent<ShopPanel>();
        if (shopPanel != null)
        {
            shopPanel.closeButton = closeBtn.GetComponent<Button>();
            Debug.Log($"[CleanupShopPanel] Wired closeButton: {shopPanel.closeButton != null}");
            EditorUtility.SetDirty(shopPanel);
        }

        // 6. Ensure CanvasGroup exists
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = panel.AddComponent<CanvasGroup>();
        }
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // 7. Panel must start ACTIVE so Awake() runs (PanelBase.Awake deactivates it)
        panel.SetActive(true);

        EditorUtility.SetDirty(panel);

        // 8. Save the scene
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[CleanupShopPanel] Cleanup complete! Header + CloseButton created, scene saved.");
    }
}
