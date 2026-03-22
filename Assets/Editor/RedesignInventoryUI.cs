using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor script to redesign the InventoryHUD with Rustic UI styling
/// and set up the SeedQuickbar at the bottom of the screen.
/// </summary>
public class RedesignInventoryUI : Editor
{
    [MenuItem("Tools/Redesign Inventory and Seed Quickbar")]
    static void Execute()
    {
        // Fix sprite import settings first
        FixSpriteImport("Assets/UI/InventoryItemSlot.png", 8, 8, 8, 8);
        FixSpriteImport("Assets/UI/InventoryPanel_BG.png", 12, 12, 12, 12);

        var canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("RedesignInventory: Canvas not found in scene!");
            return;
        }

        // Part 1: Redesign Inventory Panel
        RedesignInventoryPanel(canvas.transform);

        // Part 2: Style InventoryCell prefab
        StyleCellPrefab();

        // Part 3: Setup Seed Quickbar
        SetupSeedQuickbar(canvas.transform);

        // Save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("=== Inventory UI Redesign + Seed Quickbar Setup Complete ===");
    }

    // ======================= PART 1: INVENTORY PANEL =======================

    static void RedesignInventoryPanel(Transform canvas)
    {
        var inventoryHUD = canvas.Find("InventoryHUD");
        if (inventoryHUD == null)
        {
            Debug.LogWarning("RedesignInventory: InventoryHUD not found under Canvas!");
            return;
        }

        // --- Panel Background ---
        var panelImage = inventoryHUD.GetComponent<Image>();
        if (panelImage != null)
        {
            // Use Rustic UI - 1 (brown panel with dark border, 9-slice ready)
            var panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/Rustic UI/UI-Singles/UI - 1.png");
            if (panelSprite != null)
            {
                panelImage.sprite = panelSprite;
                panelImage.type = Image.Type.Sliced;
                panelImage.pixelsPerUnitMultiplier = 3f;
            }
            panelImage.color = new Color(0.82f, 0.72f, 0.58f, 0.97f);
        }

        // Make sure RectTransform is properly sized
        var panelRect = inventoryHUD.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.sizeDelta = new Vector2(340, 460);
        }

        // --- Header Styling ---
        var header = inventoryHUD.Find("Header");
        if (header != null)
        {
            StyleHeader(header);
        }

        // --- Scroll Area ---
        var scrollArea = inventoryHUD.Find("RecyclableScroll");
        if (scrollArea != null)
        {
            StyleScrollArea(scrollArea, inventoryHUD);
        }

        // --- Close Button ---
        SetupCloseButton(inventoryHUD);

        EditorUtility.SetDirty(inventoryHUD.gameObject);
        Debug.Log("RedesignInventory: Panel styled successfully");
    }

    static void StyleHeader(Transform header)
    {
        // Header background
        var headerImage = header.GetComponent<Image>();
        if (headerImage == null) headerImage = header.gameObject.AddComponent<Image>();

        var headerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Rustic UI/UI-Singles/UI - 55.png");
        if (headerSprite != null)
        {
            headerImage.sprite = headerSprite;
            headerImage.type = Image.Type.Sliced;
            headerImage.pixelsPerUnitMultiplier = 3f;
        }
        headerImage.color = new Color(0.65f, 0.5f, 0.35f, 1f);

        // Header RectTransform — anchor to top, height 40
        var headerRect = header.GetComponent<RectTransform>();
        if (headerRect != null)
        {
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = new Vector2(0, 0);
            headerRect.sizeDelta = new Vector2(0, 42);
        }

        // Title text styling
        var titleText = header.Find("TitleText");
        if (titleText != null)
        {
            var tmp = titleText.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.color = new Color(1f, 0.95f, 0.78f, 1f);
                tmp.fontSize = 18;
                tmp.fontStyle = FontStyles.Bold;
                tmp.alignment = TextAlignmentOptions.Center;
            }

            // Center title in header
            var titleRect = titleText.GetComponent<RectTransform>();
            if (titleRect != null)
            {
                titleRect.anchorMin = Vector2.zero;
                titleRect.anchorMax = Vector2.one;
                titleRect.offsetMin = new Vector2(10, 0);
                titleRect.offsetMax = new Vector2(-10, 0);
            }
        }
    }

    static void StyleScrollArea(Transform scrollArea, Transform panel)
    {
        // Scroll area positioning — below header, above bottom padding
        var scrollRect = scrollArea.GetComponent<RectTransform>();
        if (scrollRect != null)
        {
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(8, 8);    // left, bottom padding
            scrollRect.offsetMax = new Vector2(-8, -48); // right, top padding (below header)
        }

        // Subtle inner panel background
        var scrollImage = scrollArea.GetComponent<Image>();
        if (scrollImage != null)
        {
            scrollImage.color = new Color(0.15f, 0.1f, 0.05f, 0.25f);
        }

        // Viewport — transparent
        var viewport = scrollArea.Find("Viewport");
        if (viewport != null)
        {
            var vpImage = viewport.GetComponent<Image>();
            if (vpImage != null)
            {
                vpImage.color = new Color(1, 1, 1, 0.01f); // nearly transparent (needed for mask)
            }
        }
    }

    static void SetupCloseButton(Transform panel)
    {
        var existing = panel.Find("CloseButton");
        if (existing != null)
        {
            Debug.Log("RedesignInventory: CloseButton already exists, skipping");
            return;
        }

        // Create close button
        var closeBtnGO = new GameObject("CloseButton",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        closeBtnGO.transform.SetParent(panel, false);

        var btnRect = closeBtnGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.anchoredPosition = new Vector2(-4, -4);
        btnRect.sizeDelta = new Vector2(30, 30);

        var btnImage = closeBtnGO.GetComponent<Image>();
        var btnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Rustic UI/UI-Singles/UI - 15.png");
        if (btnSprite != null)
        {
            btnImage.sprite = btnSprite;
            btnImage.type = Image.Type.Sliced;
            btnImage.pixelsPerUnitMultiplier = 3f;
        }
        btnImage.color = new Color(0.75f, 0.25f, 0.2f, 1f);

        // X text
        var xGO = new GameObject("XText",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        xGO.transform.SetParent(closeBtnGO.transform, false);

        var xRect = xGO.GetComponent<RectTransform>();
        xRect.anchorMin = Vector2.zero;
        xRect.anchorMax = Vector2.one;
        xRect.offsetMin = Vector2.zero;
        xRect.offsetMax = Vector2.zero;

        var xTMP = xGO.GetComponent<TextMeshProUGUI>();
        xTMP.text = "X";
        xTMP.fontSize = 16;
        xTMP.fontStyle = FontStyles.Bold;
        xTMP.color = Color.white;
        xTMP.alignment = TextAlignmentOptions.Center;

        Debug.Log("RedesignInventory: Close button created");
    }

    // ======================= PART 2: CELL PREFAB =======================

    static void StyleCellPrefab()
    {
        string prefabPath = "Assets/Prefabs/InventoryCell.prefab";
        var prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null)
        {
            Debug.LogWarning("RedesignInventory: InventoryCell.prefab not found!");
            return;
        }

        // Cell background — assign InventoryItemSlot sprite
        var cellImage = prefabRoot.GetComponent<Image>();
        if (cellImage != null)
        {
            var slotSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
                "Assets/UI/InventoryItemSlot.png");
            if (slotSprite != null)
            {
                cellImage.sprite = slotSprite;
                cellImage.type = Image.Type.Sliced;
                cellImage.pixelsPerUnitMultiplier = 3f;
            }
            cellImage.color = new Color(0.78f, 0.68f, 0.55f, 0.92f);
        }

        // Increase cell height for better readability
        var cellRect = prefabRoot.GetComponent<RectTransform>();
        if (cellRect != null)
        {
            cellRect.sizeDelta = new Vector2(0, 66);
        }

        // NameLabel — dark brown, bold
        var nameLabel = prefabRoot.transform.Find("NameLabel");
        if (nameLabel != null)
        {
            var nameText = nameLabel.GetComponent<Text>();
            if (nameText != null)
            {
                nameText.color = new Color(0.2f, 0.12f, 0.05f, 1f);
                nameText.fontSize = 14;
                nameText.fontStyle = FontStyle.Bold;
            }
        }

        // DescLabel — medium brown
        var descLabel = prefabRoot.transform.Find("DescLabel");
        if (descLabel != null)
        {
            var descText = descLabel.GetComponent<Text>();
            if (descText != null)
            {
                descText.color = new Color(0.4f, 0.3f, 0.2f, 1f);
                descText.fontSize = 11;
            }
        }

        // QtyLabel — golden yellow
        var qtyLabel = prefabRoot.transform.Find("QtyLabel");
        if (qtyLabel != null)
        {
            var qtyTMP = qtyLabel.GetComponent<TMP_Text>();
            if (qtyTMP != null)
            {
                qtyTMP.color = new Color(0.9f, 0.75f, 0.1f, 1f);
                qtyTMP.fontSize = 13;
                qtyTMP.fontStyle = FontStyles.Bold;
            }
        }

        // Highlight — warm golden glow
        var highlight = prefabRoot.transform.Find("Highlight");
        if (highlight != null)
        {
            var hlImage = highlight.GetComponent<Image>();
            if (hlImage != null)
            {
                hlImage.color = new Color(1f, 0.85f, 0.3f, 0.25f);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabRoot);
        Debug.Log("RedesignInventory: InventoryCell prefab styled");
    }

    // ======================= PART 3: SEED QUICKBAR =======================

    static void SetupSeedQuickbar(Transform canvas)
    {
        // Check if SeedQuickbar already exists
        var existing = canvas.Find("SeedQuickbar");
        if (existing != null)
        {
            Debug.Log("RedesignInventory: SeedQuickbar already exists, updating icons only");
            WireSeedIcons(existing.GetComponent<SeedQuickbarUI>());
            return;
        }

        // Create SeedQuickbar panel
        var quickbarGO = new GameObject("SeedQuickbar",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(SeedQuickbarUI));
        quickbarGO.transform.SetParent(canvas, false);

        // Position: bottom-center, above tool quickbar
        var qbRect = quickbarGO.GetComponent<RectTransform>();
        qbRect.anchorMin = new Vector2(0.5f, 0);
        qbRect.anchorMax = new Vector2(0.5f, 0);
        qbRect.pivot = new Vector2(0.5f, 0);
        qbRect.anchoredPosition = new Vector2(0, 85); // above tool quickbar
        qbRect.sizeDelta = new Vector2(300, 64);

        // Background image
        var bgImage = quickbarGO.GetComponent<Image>();
        var bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Rustic UI/UI-Singles/UI - 1.png");
        if (bgSprite != null)
        {
            bgImage.sprite = bgSprite;
            bgImage.type = Image.Type.Sliced;
            bgImage.pixelsPerUnitMultiplier = 3f;
        }
        bgImage.color = new Color(0.4f, 0.55f, 0.35f, 0.88f);

        // Label above the bar
        var labelGO = new GameObject("Label",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelGO.transform.SetParent(quickbarGO.transform, false);

        var labelRect = labelGO.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 1);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.pivot = new Vector2(0.5f, 0);
        labelRect.anchoredPosition = new Vector2(0, 2);
        labelRect.sizeDelta = new Vector2(0, 16);

        var labelTMP = labelGO.GetComponent<TextMeshProUGUI>();
        labelTMP.text = "Hạt giống (5-9)";
        labelTMP.fontSize = 10;
        labelTMP.alignment = TextAlignmentOptions.Center;
        labelTMP.color = new Color(0.9f, 1f, 0.85f, 0.75f);

        // Wire SeedQuickbarUI component
        var seedQuickbar = quickbarGO.GetComponent<SeedQuickbarUI>();
        seedQuickbar.slotContainer = quickbarGO.transform;
        seedQuickbar.slotSize = new Vector2(52, 52);
        seedQuickbar.slotSpacing = 4f;
        seedQuickbar.padding = 6f;

        // Wire seed icons
        WireSeedIcons(seedQuickbar);

        EditorUtility.SetDirty(quickbarGO);
        Debug.Log("RedesignInventory: SeedQuickbar created and configured");
    }

    /// <summary>
    /// Wires seed icon sprites to the SeedQuickbarUI component.
    /// Scans Assets/UI/Seed_*.png for available seed icons.
    /// </summary>
    static void WireSeedIcons(SeedQuickbarUI quickbar)
    {
        if (quickbar == null) return;

        // Find all seed icon sprites
        string[] seedIconGuids = AssetDatabase.FindAssets("Seed_ t:Sprite", new[] { "Assets/UI" });
        var entries = new System.Collections.Generic.List<SeedQuickbarUI.SeedIconEntry>();

        foreach (string guid in seedIconGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) continue;

            // Extract seed name: "Assets/UI/Seed_Tomato.png" → "tomato"
            string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (fileName.StartsWith("Seed_"))
            {
                string seedName = fileName.Substring(5).ToLower();
                var entry = new SeedQuickbarUI.SeedIconEntry
                {
                    itemIdPrefix = seedName,
                    icon = sprite
                };
                entries.Add(entry);
                Debug.Log($"  Mapped seed icon: {seedName} → {path}");
            }
        }

        // Use SerializedObject to properly set the array
        var so = new SerializedObject(quickbar);
        var prop = so.FindProperty("seedIcons");
        prop.arraySize = entries.Count;

        for (int i = 0; i < entries.Count; i++)
        {
            var element = prop.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("itemIdPrefix").stringValue = entries[i].itemIdPrefix;
            element.FindPropertyRelative("icon").objectReferenceValue = entries[i].icon;
        }

        so.ApplyModifiedProperties();
        Debug.Log($"RedesignInventory: Wired {entries.Count} seed icons");
    }

    // ======================= UTILITY =======================

    /// <summary>
    /// Fixes a sprite's import settings for proper 9-slice usage in UI.
    /// </summary>
    static void FixSpriteImport(string path, int borderL, int borderB, int borderR, int borderT)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"RedesignInventory: TextureImporter not found at {path}");
            return;
        }

        bool changed = false;

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        var newBorder = new Vector4(borderL, borderB, borderR, borderT);
        if (importer.spriteBorder != newBorder)
        {
            importer.spriteBorder = newBorder;
            changed = true;
        }

        if (importer.filterMode != FilterMode.Point)
        {
            importer.filterMode = FilterMode.Point;
            changed = true;
        }

        if (Mathf.Abs(importer.spritePixelsPerUnit - 16f) > 0.01f)
        {
            importer.spritePixelsPerUnit = 16f;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
            Debug.Log($"RedesignInventory: Fixed sprite import settings for {path}");
        }
    }
}
