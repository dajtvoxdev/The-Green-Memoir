using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

public class CreateShopPanel
{
    [MenuItem("MoonlitGarden/Create Shop Panel")]
    public static void Run()
    {
        // Find Canvas
        var canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) { Debug.LogError("No Canvas found!"); return; }

        // Create ShopPanel root
        var panelGO = new GameObject("ShopPanel");
        panelGO.transform.SetParent(canvas.transform, false);

        var rt = panelGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.15f, 0.1f);
        rt.anchorMax = new Vector2(0.85f, 0.9f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // CanvasGroup (required by PanelBase)
        var cg = panelGO.AddComponent<CanvasGroup>();
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        // Background image
        var bgImage = panelGO.AddComponent<Image>();
        bgImage.color = new Color(0.12f, 0.08f, 0.05f, 0.95f);

        // Add ShopPanel script
        var shopPanel = panelGO.AddComponent<ShopPanel>();
        shopPanel.panelId = "shop";
        shopPanel.closeOnEscape = true;
        shopPanel.pauseGameWhenOpen = true;
        shopPanel.fadeSpeed = 8f;

        // --- Header bar ---
        var header = CreateChild(panelGO.transform, "Header");
        SetAnchors(header, new Vector2(0, 0.88f), Vector2.one, Vector2.zero, Vector2.zero);
        var headerImg = header.AddComponent<Image>();
        headerImg.color = new Color(0.18f, 0.12f, 0.07f, 1f);

        // Close button (only UI element needed in header — gold is shown in GoldHUD)
        var closeGO = CreateChild(header.transform, "CloseButton");
        SetAnchors(closeGO, new Vector2(0.92f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
        var closeImg = closeGO.AddComponent<Image>();
        closeImg.color = new Color(0.7f, 0.2f, 0.2f, 1f);
        var closeBtn = closeGO.AddComponent<Button>();
        shopPanel.closeButton = closeBtn;
        var closeTxt = CreateTMP(closeGO.transform, "CloseText", "X", 20, TextAlignmentOptions.Center);
        SetAnchors(closeTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // --- Tab bar ---
        var tabBar = CreateChild(panelGO.transform, "TabBar");
        SetAnchors(tabBar, new Vector2(0, 0.8f), new Vector2(1, 0.88f), Vector2.zero, Vector2.zero);
        var tabBarImg = tabBar.AddComponent<Image>();
        tabBarImg.color = new Color(0.15f, 0.1f, 0.06f, 1f);

        // Buy tab
        var buyTabGO = CreateChild(tabBar.transform, "BuyTab");
        SetAnchors(buyTabGO, new Vector2(0.05f, 0.1f), new Vector2(0.48f, 0.9f), Vector2.zero, Vector2.zero);
        var buyTabImg = buyTabGO.AddComponent<Image>();
        buyTabImg.color = new Color(0.3f, 0.5f, 0.3f, 1f);
        var buyTabBtn = buyTabGO.AddComponent<Button>();
        shopPanel.buyTabButton = buyTabBtn;
        var buyTabTxt = CreateTMP(buyTabGO.transform, "BuyTabText", "Mua", 18, TextAlignmentOptions.Center);
        SetAnchors(buyTabTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Sell tab
        var sellTabGO = CreateChild(tabBar.transform, "SellTab");
        SetAnchors(sellTabGO, new Vector2(0.52f, 0.1f), new Vector2(0.95f, 0.9f), Vector2.zero, Vector2.zero);
        var sellTabImg = sellTabGO.AddComponent<Image>();
        sellTabImg.color = new Color(0.5f, 0.35f, 0.2f, 1f);
        var sellTabBtn = sellTabGO.AddComponent<Button>();
        shopPanel.sellTabButton = sellTabBtn;
        var sellTabTxt = CreateTMP(sellTabGO.transform, "SellTabText", "Ban", 18, TextAlignmentOptions.Center);
        SetAnchors(sellTabTxt, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // --- Item container with scroll ---
        var scrollArea = CreateChild(panelGO.transform, "ScrollArea");
        SetAnchors(scrollArea, new Vector2(0.02f, 0.02f), new Vector2(0.98f, 0.8f), Vector2.zero, Vector2.zero);
        var scrollImg = scrollArea.AddComponent<Image>();
        scrollImg.color = new Color(0.08f, 0.06f, 0.04f, 0.8f);
        var scrollRect = scrollArea.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        // Viewport
        var viewport = CreateChild(scrollArea.transform, "Viewport");
        SetAnchors(viewport, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = viewport.GetComponent<RectTransform>();

        // Content (item container)
        var content = CreateChild(viewport.transform, "Content");
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.offsetMin = new Vector2(0, 0);
        contentRT.offsetMax = new Vector2(0, 0);

        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = contentRT;
        shopPanel.itemContainer = content.transform;

        // Start hidden
        panelGO.SetActive(false);

        // Register with UIManager
        var uiManager = Object.FindFirstObjectByType<UIManager>();
        if (uiManager != null)
        {
            // Add to panels array
            var panelsList = new System.Collections.Generic.List<PanelBase>();
            if (uiManager.panels != null)
                panelsList.AddRange(uiManager.panels);
            panelsList.Add(shopPanel);
            uiManager.panels = panelsList.ToArray();
            EditorUtility.SetDirty(uiManager);
            Debug.Log("[CreateShopPanel] ShopPanel registered with UIManager");
        }

        EditorUtility.SetDirty(panelGO);
        Debug.Log("[CreateShopPanel] ShopPanel created successfully under Canvas!");
    }

    private static GameObject CreateChild(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static void SetAnchors(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    private static GameObject CreateTMP(Transform parent, string name, string text, float fontSize, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        return go;
    }
}
