using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using PolyAndCode.UI;

/// <summary>
/// One-shot editor utility: creates the Content RectTransform and InventoryCell prefab
/// needed by RecyclableScrollRect, then wires them up in the scene.
/// Run via: Tools > Setup Inventory UI
/// </summary>
public static class SetupInventoryUI
{
    [MenuItem("Tools/Setup Inventory UI")]
    public static void Run()
    {
        // 1. Find RecyclableScroll in scene
        var scrollGO = GameObject.Find("RecyclableScroll");
        if (scrollGO == null) { Debug.LogError("[SetupInventoryUI] RecyclableScroll not found!"); return; }

        var scrollRect = scrollGO.GetComponent<RecyclableScrollRect>();
        if (scrollRect == null) { Debug.LogError("[SetupInventoryUI] RecyclableScrollRect not found!"); return; }

        // 2. Create Content child with RectTransform
        GameObject contentGO = null;
        for (int i = 0; i < scrollGO.transform.childCount; i++)
        {
            if (scrollGO.transform.GetChild(i).name == "Content")
            {
                contentGO = scrollGO.transform.GetChild(i).gameObject;
                break;
            }
        }
        if (contentGO == null)
        {
            contentGO = new GameObject("Content");
            contentGO.transform.SetParent(scrollGO.transform, false);
        }

        var contentRT = contentGO.GetComponent<RectTransform>();
        if (contentRT == null) contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 0);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;
        contentRT.pivot = new Vector2(0.5f, 1f);

        var vlg = contentGO.GetComponent<VerticalLayoutGroup>() ?? contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 2f;
        vlg.padding = new RectOffset(4, 4, 4, 4);

        var csf = contentGO.GetComponent<ContentSizeFitter>() ?? contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 3. Wire m_Content
        var so = new SerializedObject(scrollRect);
        so.FindProperty("m_Content").objectReferenceValue = contentRT;
        so.FindProperty("SelfInitialize").boolValue = false;
        so.ApplyModifiedProperties();
        Debug.Log("[SetupInventoryUI] Content RectTransform created and wired to RecyclableScrollRect.");

        // 4. Create InventoryCell prefab
        const string prefabFolder = "Assets/Prefabs";
        const string prefabPath   = prefabFolder + "/InventoryCell.prefab";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        var cellGO = new GameObject("InventoryCell");
        var cellRT = cellGO.AddComponent<RectTransform>();
        cellRT.sizeDelta = new Vector2(0, 60);

        var bg = cellGO.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.1f, 0.05f, 0.85f);

        var cellData = cellGO.AddComponent<CelllItemData>();

        // Icon
        var iconGO = new GameObject("Icon"); iconGO.transform.SetParent(cellGO.transform, false);
        var iconRT = iconGO.AddComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0); iconRT.anchorMax = new Vector2(0, 1);
        iconRT.offsetMin = new Vector2(4, 4); iconRT.offsetMax = new Vector2(56, -4);
        var iconImg = iconGO.AddComponent<Image>(); iconImg.color = Color.white;

        // Name
        var nameGO = new GameObject("NameLabel"); nameGO.transform.SetParent(cellGO.transform, false);
        var nameRT = nameGO.AddComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 0.5f); nameRT.anchorMax = new Vector2(1, 1);
        nameRT.offsetMin = new Vector2(64, 0);   nameRT.offsetMax = new Vector2(-8, -4);
        var nameText = nameGO.AddComponent<Text>();
        nameText.text = "Item"; nameText.fontSize = 14;
        nameText.color = new Color(0.95f, 0.85f, 0.6f);
        nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Description
        var desGO = new GameObject("DescLabel"); desGO.transform.SetParent(cellGO.transform, false);
        var desRT = desGO.AddComponent<RectTransform>();
        desRT.anchorMin = new Vector2(0, 0); desRT.anchorMax = new Vector2(1, 0.5f);
        desRT.offsetMin = new Vector2(64, 4); desRT.offsetMax = new Vector2(-8, 0);
        var desText = desGO.AddComponent<Text>();
        desText.text = ""; desText.fontSize = 11;
        desText.color = new Color(0.7f, 0.65f, 0.55f);
        desText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Quantity badge
        var qtyGO = new GameObject("QtyLabel"); qtyGO.transform.SetParent(cellGO.transform, false);
        var qtyRT = qtyGO.AddComponent<RectTransform>();
        qtyRT.anchorMin = new Vector2(1, 0); qtyRT.anchorMax = new Vector2(1, 0);
        qtyRT.pivot = new Vector2(1, 0); qtyRT.anchoredPosition = new Vector2(-4, 4);
        qtyRT.sizeDelta = new Vector2(36, 20);
        var qtyText = qtyGO.AddComponent<TextMeshProUGUI>();
        qtyText.text = "99"; qtyText.fontSize = 12;
        qtyText.color = Color.yellow;
        qtyText.alignment = TextAlignmentOptions.BottomRight;

        // Highlight overlay
        var hlGO = new GameObject("Highlight"); hlGO.transform.SetParent(cellGO.transform, false);
        var hlRT = hlGO.AddComponent<RectTransform>();
        hlRT.anchorMin = Vector2.zero; hlRT.anchorMax = Vector2.one;
        hlRT.offsetMin = Vector2.zero; hlRT.offsetMax = Vector2.zero;
        var hlImg = hlGO.AddComponent<Image>();
        hlImg.color = new Color(1f, 1f, 0.5f, 0.2f);
        hlImg.enabled = false;

        cellData.nameLabel    = nameText;
        cellData.desLabel     = desText;
        cellData.quantityLabel = qtyText;
        cellData.iconImage    = iconImg;
        cellData.highlightImage = hlImg;

        bool prefabSuccess;
        PrefabUtility.SaveAsPrefabAsset(cellGO, prefabPath, out prefabSuccess);
        Object.DestroyImmediate(cellGO);

        if (!prefabSuccess) { Debug.LogError("[SetupInventoryUI] Failed to save InventoryCell prefab!"); return; }

        // 5. Wire PrototypeCell
        var cellPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        so = new SerializedObject(scrollRect);
        so.FindProperty("PrototypeCell").objectReferenceValue = cellPrefab;
        so.ApplyModifiedProperties();

        // 6. Wire inventoryGameObject on RecyclableInventoryManager
        var invMgr = Object.FindObjectOfType<RecyclableInventoryManager>();
        if (invMgr != null)
        {
            var invHUD = GameObject.Find("InventoryHUD");
            if (invHUD != null)
            {
                var soInv = new SerializedObject(invMgr);
                soInv.FindProperty("inventoryGameObject").objectReferenceValue = invHUD;
                soInv.ApplyModifiedProperties();
                Debug.Log("[SetupInventoryUI] inventoryGameObject wired to InventoryHUD.");
            }
        }

        // 7. Mark scene dirty & save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[SetupInventoryUI] Setup complete! InventoryCell prefab: " + prefabPath);
    }
}
