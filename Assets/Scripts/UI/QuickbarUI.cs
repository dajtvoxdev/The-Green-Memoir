using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Quickbar HUD — visual toolbar showing equipped tools with key bindings.
/// Replaces the simple ToolHUD with a Stardew Valley-style horizontal slot bar.
///
/// Phase 2 Feature (#27): Quickbar UI + input binding.
///
/// Auto-generates slots from EquipmentManager.starterTools if no prefab is provided.
/// Subscribes to EquipmentManager.OnToolChanged to highlight the active slot.
///
/// Usage: Attach to a UI panel anchored at bottom-center of Canvas.
/// </summary>
public class QuickbarUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Parent transform for quickbar slots. If null, uses this transform.")]
    public Transform slotContainer;

    [Tooltip("Optional prefab for each slot. If null, slots are generated programmatically.")]
    public GameObject slotPrefab;

    [Header("Layout")]
    [Tooltip("Size of each slot in pixels.")]
    public Vector2 slotSize = new Vector2(64, 64);

    [Tooltip("Spacing between slots.")]
    public float slotSpacing = 4f;

    [Tooltip("Padding around the quickbar.")]
    public float padding = 8f;

    private readonly List<QuickbarSlot> slots = new List<QuickbarSlot>();
    private int activeSlotIndex = -1;

    void Start()
    {
        if (slotContainer == null)
        {
            slotContainer = transform;
        }

        BuildQuickbar();

        // Subscribe to tool changes
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnToolChanged += OnToolChanged;

            // Set initial selection
            OnToolChanged(EquipmentManager.Instance.CurrentTool);
        }
    }

    /// <summary>
    /// Builds the quickbar slots from EquipmentManager's starter tools.
    /// </summary>
    private void BuildQuickbar()
    {
        // Clear existing slots
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }
        slots.Clear();

        ToolDefinition[] tools = EquipmentManager.Instance?.starterTools;
        int slotCount = tools != null ? Mathf.Max(tools.Length, 4) : 4;
        slotCount = Mathf.Min(slotCount, 6); // Max 6 slots

        // Ensure container has HorizontalLayoutGroup
        EnsureLayoutGroup();

        for (int i = 0; i < slotCount; i++)
        {
            ToolDefinition tool = (tools != null && i < tools.Length) ? tools[i] : null;
            QuickbarSlot slot = CreateSlot(i, tool);
            slots.Add(slot);
        }

        Debug.Log($"QuickbarUI: Built {slotCount} slots");
    }

    /// <summary>
    /// Creates a single quickbar slot.
    /// Uses slotPrefab if available, otherwise generates UI programmatically.
    /// </summary>
    private QuickbarSlot CreateSlot(int index, ToolDefinition tool)
    {
        GameObject slotGO;

        if (slotPrefab != null)
        {
            slotGO = Instantiate(slotPrefab, slotContainer);
        }
        else
        {
            slotGO = GenerateSlotUI(index);
        }

        slotGO.name = $"Slot_{index + 1}";

        QuickbarSlot slot = slotGO.GetComponent<QuickbarSlot>();
        if (slot == null)
        {
            slot = slotGO.AddComponent<QuickbarSlot>();
        }

        // Wire up references if generated
        if (slotPrefab == null)
        {
            WireSlotReferences(slot, slotGO);
        }

        slot.Setup(tool, index);
        return slot;
    }

    /// <summary>
    /// Generates a slot UI element programmatically (no prefab needed).
    /// Structure: SlotRoot (Image bg) > Icon (Image) + KeyLabel (TMP) + NameLabel (TMP)
    /// </summary>
    private GameObject GenerateSlotUI(int index)
    {
        // Root slot object with background
        GameObject root = new GameObject($"Slot_{index + 1}", typeof(RectTransform), typeof(Image));
        root.transform.SetParent(slotContainer, false);

        RectTransform rootRT = root.GetComponent<RectTransform>();
        rootRT.sizeDelta = slotSize;

        Image rootBg = root.GetComponent<Image>();
        rootBg.color = new Color(0.2f, 0.2f, 0.2f, 0.7f);

        // Icon image (centered, slightly smaller than slot)
        GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(root.transform, false);

        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.15f, 0.25f);
        iconRT.anchorMax = new Vector2(0.85f, 0.85f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        Image iconImg = iconGO.GetComponent<Image>();
        iconImg.preserveAspect = true;
        iconImg.enabled = false; // Hidden until tool is assigned

        // Key number label (top-left corner)
        GameObject keyGO = new GameObject("KeyLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        keyGO.transform.SetParent(root.transform, false);

        RectTransform keyRT = keyGO.GetComponent<RectTransform>();
        keyRT.anchorMin = new Vector2(0, 0.7f);
        keyRT.anchorMax = new Vector2(0.4f, 1f);
        keyRT.offsetMin = Vector2.zero;
        keyRT.offsetMax = Vector2.zero;

        TextMeshProUGUI keyTMP = keyGO.GetComponent<TextMeshProUGUI>();
        keyTMP.text = (index + 1).ToString();
        keyTMP.fontSize = 10;
        keyTMP.alignment = TextAlignmentOptions.TopLeft;
        keyTMP.color = new Color(1f, 1f, 1f, 0.6f);
        keyTMP.margin = new Vector4(3, 0, 0, 0);

        // Name label (bottom, small text)
        GameObject nameGO = new GameObject("NameLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
        nameGO.transform.SetParent(root.transform, false);

        RectTransform nameRT = nameGO.GetComponent<RectTransform>();
        nameRT.anchorMin = new Vector2(0, 0);
        nameRT.anchorMax = new Vector2(1, 0.25f);
        nameRT.offsetMin = Vector2.zero;
        nameRT.offsetMax = Vector2.zero;

        TextMeshProUGUI nameTMP = nameGO.GetComponent<TextMeshProUGUI>();
        nameTMP.fontSize = 8;
        nameTMP.alignment = TextAlignmentOptions.Bottom;
        nameTMP.color = new Color(1f, 1f, 1f, 0.8f);
        nameTMP.overflowMode = TextOverflowModes.Ellipsis;

        return root;
    }

    /// <summary>
    /// Wires QuickbarSlot component references for programmatically generated slots.
    /// </summary>
    private void WireSlotReferences(QuickbarSlot slot, GameObject slotGO)
    {
        slot.backgroundImage = slotGO.GetComponent<Image>();

        Transform iconT = slotGO.transform.Find("Icon");
        if (iconT != null)
        {
            slot.iconImage = iconT.GetComponent<Image>();
        }

        Transform keyT = slotGO.transform.Find("KeyLabel");
        if (keyT != null)
        {
            slot.keyText = keyT.GetComponent<TMP_Text>();
        }

        Transform nameT = slotGO.transform.Find("NameLabel");
        if (nameT != null)
        {
            slot.nameText = nameT.GetComponent<TMP_Text>();
        }
    }

    /// <summary>
    /// Ensures the slot container has a HorizontalLayoutGroup for auto-layout.
    /// </summary>
    private void EnsureLayoutGroup()
    {
        HorizontalLayoutGroup hlg = slotContainer.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
        {
            hlg = slotContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
        }

        hlg.spacing = slotSpacing;
        hlg.padding = new RectOffset(
            Mathf.RoundToInt(padding),
            Mathf.RoundToInt(padding),
            Mathf.RoundToInt(padding),
            Mathf.RoundToInt(padding)
        );
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // Add ContentSizeFitter for dynamic width
        ContentSizeFitter csf = slotContainer.GetComponent<ContentSizeFitter>();
        if (csf == null)
        {
            csf = slotContainer.gameObject.AddComponent<ContentSizeFitter>();
        }
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    /// <summary>
    /// Handles tool change events — highlights the matching slot.
    /// </summary>
    private void OnToolChanged(ToolDefinition tool)
    {
        // Deselect previous
        if (activeSlotIndex >= 0 && activeSlotIndex < slots.Count)
        {
            slots[activeSlotIndex].SetSelected(false);
        }

        activeSlotIndex = -1;

        if (tool == null) return;

        // Find and select the matching slot
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].AssignedTool == tool)
            {
                activeSlotIndex = i;
                slots[i].SetSelected(true);
                break;
            }
        }
    }

    /// <summary>
    /// Rebuilds the quickbar (e.g., after tools change at runtime).
    /// </summary>
    public void Refresh()
    {
        BuildQuickbar();

        if (EquipmentManager.Instance != null)
        {
            OnToolChanged(EquipmentManager.Instance.CurrentTool);
        }
    }

    void OnDestroy()
    {
        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnToolChanged -= OnToolChanged;
        }
    }
}
