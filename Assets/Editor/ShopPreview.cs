using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Creates preview item cells in the ShopPanel for editor layout adjustment.
/// Run via: MoonlitGarden > Shop Preview > Show Items
/// Clean via: MoonlitGarden > Shop Preview > Clear Items
/// </summary>
public class ShopPreview
{
    [MenuItem("MoonlitGarden/Shop Preview/Show Items")]
    public static void ShowPreviewItems()
    {
        var content = FindContent();
        if (content == null) { Debug.LogError("Content not found"); return; }

        // Clear existing preview
        ClearItems();

        // Remove any existing layout groups that conflict with GridLayoutGroup
        var vlg = content.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) Object.DestroyImmediate(vlg);
        var hlg = content.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null) Object.DestroyImmediate(hlg);

        // Setup grid
        var grid = content.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = content.gameObject.AddComponent<GridLayoutGroup>();
        grid.padding = new RectOffset(8, 8, 8, 8);
        grid.spacing = new Vector2(8, 8);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4;
        grid.cellSize = new Vector2(90, 100);
        grid.childAlignment = TextAnchor.UpperCenter;

        // Load catalog
        var catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>("Assets/Data/Shop/GeneralStore.asset");
        if (catalog == null || catalog.entries == null)
        {
            Debug.LogError("GeneralStore catalog not found");
            return;
        }

        int count = 0;
        foreach (var entry in catalog.entries)
        {
            if (entry == null || entry.item == null) continue;

            // Cell background
            var cell = new GameObject("Preview_" + entry.item.itemName);
            cell.transform.SetParent(content, false);
            var cellImg = cell.AddComponent<Image>();
            cellImg.color = new Color(0.1f, 0.08f, 0.05f, 0.95f);
            var outline = cell.AddComponent<Outline>();
            outline.effectColor = new Color(0.45f, 0.38f, 0.25f, 0.8f);

            // Icon (top 70%)
            var iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(cell.transform, false);
            var iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.05f, 0.30f);
            iconRT.anchorMax = new Vector2(0.95f, 0.95f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            var iconImg = iconGO.AddComponent<Image>();
            if (entry.item.icon != null)
            {
                iconImg.sprite = entry.item.icon;
                iconImg.preserveAspect = true;
            }
            else
            {
                iconImg.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            }

            // Price label (bottom 28%)
            var priceGO = new GameObject("Price");
            priceGO.transform.SetParent(cell.transform, false);
            var priceRT = priceGO.AddComponent<RectTransform>();
            priceRT.anchorMin = new Vector2(0, 0);
            priceRT.anchorMax = new Vector2(1, 0.28f);
            priceRT.offsetMin = Vector2.zero;
            priceRT.offsetMax = Vector2.zero;
            var tmp = priceGO.AddComponent<TextMeshProUGUI>();
            tmp.text = entry.buyPrice + "G";
            tmp.fontSize = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(1f, 0.84f, 0f);

            count++;
        }

        EditorUtility.SetDirty(content.gameObject);
        Debug.Log("[ShopPreview] Created " + count + " preview cells");
    }

    [MenuItem("MoonlitGarden/Shop Preview/Clear Items")]
    public static void ClearItems()
    {
        var content = FindContent();
        if (content == null) return;

        while (content.childCount > 0)
            Object.DestroyImmediate(content.GetChild(0).gameObject);

        // Remove GridLayoutGroup added by preview
        var grid = content.GetComponent<GridLayoutGroup>();
        if (grid != null) Object.DestroyImmediate(grid);

        EditorUtility.SetDirty(content.gameObject);
        Debug.Log("[ShopPreview] Cleared all preview cells");
    }

    private static Transform FindContent()
    {
        var allRT = Resources.FindObjectsOfTypeAll<RectTransform>();
        foreach (var t in allRT)
        {
            if (t.name == "ShopPanel" && t.GetComponent<CanvasGroup>() != null)
            {
                var content = t.Find("ScrollArea/Viewport/Content");
                if (content != null) return content;
            }
        }
        return null;
    }
}
