using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Shop UI panel with grid-based item display and hover tooltips.
/// Dynamically sizes grid cells to fill the available panel space.
/// </summary>
public class ShopPanel : PanelBase
{
    [Header("Shop UI References")]
    public Transform itemContainer;
    public Button buyTabButton;
    public Button sellTabButton;
    public Button closeButton;

    [Header("Grid Settings")]
    public int columnCount = 4;
    public float cellAspectRatio = 1.1f; // height/width ratio

    private enum ShopTab { Buy, Sell }
    private ShopTab currentTab = ShopTab.Buy;
    private readonly List<GameObject> spawnedCells = new List<GameObject>();
    private GridLayoutGroup gridLayout;

    // Tooltip
    private GameObject tooltipGO;
    private TMP_Text tooltipName;
    private TMP_Text tooltipDesc;
    private TMP_Text tooltipPrice;
    private CanvasGroup tooltipCanvasGroup;
    private RectTransform tooltipRT;

    // Tab tracking
    private Image buyTabImage;
    private Image sellTabImage;

    private static readonly Color TAB_ACTIVE = new Color(0.3f, 0.55f, 0.3f, 1f);
    private static readonly Color TAB_INACTIVE = new Color(0.2f, 0.18f, 0.15f, 0.9f);
    private static readonly Color CELL_BG = new Color(0.1f, 0.08f, 0.05f, 0.95f);
    private static readonly Color CELL_HOVER = new Color(0.28f, 0.24f, 0.16f, 1f);
    private static readonly Color CELL_DISABLED = new Color(0.1f, 0.08f, 0.05f, 0.4f);
    private static readonly Color GOLD_COLOR = new Color(1f, 0.84f, 0f);

    protected override void Awake()
    {
        panelId = "shop";
        AutoWireReferences();
        base.Awake();

        var uiManager = UIManager.Instance != null
            ? UIManager.Instance
            : Object.FindFirstObjectByType<UIManager>();
        if (uiManager != null) uiManager.RegisterPanel(this);

        if (buyTabButton != null)
        {
            buyTabImage = buyTabButton.GetComponent<Image>();
            buyTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Buy));
        }
        if (sellTabButton != null)
        {
            sellTabImage = sellTabButton.GetComponent<Image>();
            sellTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Sell));
        }
        if (closeButton != null)
            closeButton.onClick.AddListener(() =>
            {
                if (UIManager.Instance != null) UIManager.Instance.HidePanel("shop");
                else Hide();
            });

        SetupGridLayout();
        CreateTooltip();
    }

    private void AutoWireReferences()
    {
        if (closeButton == null)
        {
            var t = transform.Find("Header/CloseButton");
            if (t != null) closeButton = t.GetComponent<Button>();
        }
        if (buyTabButton == null)
        {
            var t = transform.Find("TabBar/BuyTab");
            if (t != null) buyTabButton = t.GetComponent<Button>();
        }
        if (sellTabButton == null)
        {
            var t = transform.Find("TabBar/SellTab");
            if (t != null) sellTabButton = t.GetComponent<Button>();
        }
        if (itemContainer == null)
        {
            var t = transform.Find("ScrollArea/Viewport/Content");
            if (t != null) itemContainer = t;
        }
    }

    private void SetupGridLayout()
    {
        if (itemContainer == null) return;

        var vlg = itemContainer.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) DestroyImmediate(vlg);
        var hlg = itemContainer.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null) DestroyImmediate(hlg);

        gridLayout = itemContainer.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = itemContainer.gameObject.AddComponent<GridLayoutGroup>();

        gridLayout.padding = new RectOffset(8, 8, 8, 8);
        gridLayout.spacing = new Vector2(8, 8);
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columnCount;
    }

    /// <summary>
    /// Calculates cell size dynamically based on the actual container width.
    /// </summary>
    private void RecalculateCellSize()
    {
        if (gridLayout == null || itemContainer == null) return;

        var containerRT = itemContainer as RectTransform;
        if (containerRT == null) return;

        // Force layout rebuild to get accurate rect
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

        float availableWidth = containerRT.rect.width;
        if (availableWidth <= 0) availableWidth = 500f; // fallback

        float totalSpacing = gridLayout.spacing.x * (columnCount - 1);
        float totalPadding = gridLayout.padding.left + gridLayout.padding.right;
        float cellWidth = (availableWidth - totalSpacing - totalPadding) / columnCount;
        cellWidth = Mathf.Max(cellWidth, 60f); // minimum size

        float cellHeight = cellWidth * cellAspectRatio;
        gridLayout.cellSize = new Vector2(cellWidth, cellHeight);
    }

    private void CreateTooltip()
    {
        tooltipGO = new GameObject("ShopTooltip", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        tooltipGO.transform.SetParent(transform, false);

        tooltipRT = tooltipGO.GetComponent<RectTransform>();
        tooltipRT.sizeDelta = new Vector2(240, 160);
        tooltipRT.pivot = new Vector2(0, 1);

        tooltipCanvasGroup = tooltipGO.GetComponent<CanvasGroup>();
        tooltipCanvasGroup.alpha = 0;
        tooltipCanvasGroup.blocksRaycasts = false;
        tooltipCanvasGroup.interactable = false;

        var bg = tooltipGO.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.04f, 0.02f, 0.97f);

        var outline = tooltipGO.AddComponent<Outline>();
        outline.effectColor = GOLD_COLOR;
        outline.effectDistance = new Vector2(2f, -2f);

        var layout = tooltipGO.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(14, 14, 10, 10);
        layout.spacing = 6;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;
        layout.childControlHeight = true;

        var csf = tooltipGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

        var nameObj = CreateTMPText(tooltipGO.transform, "TooltipName", "", 18,
            TextAlignmentOptions.TopLeft, Color.white);
        nameObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        tooltipName = nameObj.GetComponent<TMP_Text>();
        var nameLE = nameObj.AddComponent<LayoutElement>();
        nameLE.minWidth = 200;

        var descObj = CreateTMPText(tooltipGO.transform, "TooltipDesc", "", 14,
            TextAlignmentOptions.TopLeft, new Color(0.75f, 0.72f, 0.65f));
        descObj.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;
        tooltipDesc = descObj.GetComponent<TMP_Text>();

        var priceObj = CreateTMPText(tooltipGO.transform, "TooltipPrice", "", 16,
            TextAlignmentOptions.TopLeft, GOLD_COLOR);
        priceObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        tooltipPrice = priceObj.GetComponent<TMP_Text>();
    }

    protected override void OnShow()
    {
        UpdateTabHighlights();
        // Delay one frame so RectTransform sizes are resolved
        StartCoroutine(RefreshNextFrame());
    }

    private System.Collections.IEnumerator RefreshNextFrame()
    {
        yield return null; // wait one frame for layout
        RecalculateCellSize();
        RefreshDisplay();
    }

    protected override void OnHide()
    {
        HideTooltip();
        ShopManager.Instance?.CloseShop();
    }

    private void SwitchTab(ShopTab tab)
    {
        currentTab = tab;
        UpdateTabHighlights();
        HideTooltip();
        RefreshDisplay();
    }

    private void UpdateTabHighlights()
    {
        if (buyTabImage != null)
            buyTabImage.color = currentTab == ShopTab.Buy ? TAB_ACTIVE : TAB_INACTIVE;
        if (sellTabImage != null)
            sellTabImage.color = currentTab == ShopTab.Sell ? TAB_ACTIVE : TAB_INACTIVE;
    }

    public void RefreshDisplay()
    {
        ClearCells();
        if (currentTab == ShopTab.Buy) PopulateBuyTab();
        else PopulateSellTab();
    }

    private void PopulateBuyTab()
    {
        if (ShopManager.Instance?.CurrentCatalog?.entries == null) return;

        foreach (var entry in ShopManager.Instance.CurrentCatalog.entries)
        {
            if (entry.item == null || !entry.available) continue;
            bool canAfford = PlayerEconomyManager.Instance?.CanAffordGold(entry.buyPrice) ?? false;

            CreateItemCell(
                icon: entry.item.icon,
                itemName: entry.item.itemName,
                description: entry.item.description,
                price: entry.buyPrice,
                actionLabel: LocalizationManager.LocalizeText("Buy"),
                canTransact: canAfford && entry.CanBuy,
                onClick: () => StartCoroutine(DeferredAction(() =>
                {
                    ShopManager.Instance?.BuyItem(entry, 1);
                    RefreshDisplay();
                }))
            );
        }
    }

    private void PopulateSellTab()
    {
        var inventoryManager = ShopManager.Instance?.inventoryManager;
        if (inventoryManager == null)
        {
            Debug.LogWarning("ShopPanel: SellTab — inventoryManager is null!");
            return;
        }

        var items = inventoryManager.GetInventoryItems();
        Debug.Log($"ShopPanel: SellTab — scanning {items.Count} inventory items");
        foreach (var item in items)
        {
            // Try shop catalog first (for seeds)
            ShopEntry sellEntry = ShopManager.Instance.FindEntryByItemId(item.itemId);
            int sellPrice = sellEntry?.sellPrice ?? 0;
            Sprite icon = sellEntry?.item?.icon;

            // Fallback: check CropDefinition for harvested crops (crop_* items)
            if (sellPrice <= 0 && item.itemId != null && item.itemId.StartsWith("crop_")
                && CropGrowthManager.Instance != null)
            {
                string cropId = item.itemId.Substring(5); // strip "crop_" prefix
                var cropDef = CropGrowthManager.Instance.GetCropDefinition(cropId);
                if (cropDef != null && cropDef.sellPrice > 0)
                {
                    sellPrice = cropDef.sellPrice;
                    // Use the harvestable stage sprite as icon
                    icon = cropDef.GetStageSprite(GrowthStage.Harvestable);
                }
            }

            if (sellPrice <= 0)
            {
                Debug.Log($"ShopPanel: SellTab — skipping '{item.itemId}' (sellPrice=0)");
                continue;
            }

            Debug.Log($"ShopPanel: SellTab — showing '{item.itemId}' x{item.quantity} at {sellPrice}G");

            // Capture for closure
            int capturedPrice = sellPrice;

            CreateItemCell(
                icon: icon,
                itemName: $"{item.name} x{item.quantity}",
                description: item.description,
                price: sellPrice,
                actionLabel: LocalizationManager.LocalizeText("Sell"),
                canTransact: item.quantity > 0,
                onClick: () => StartCoroutine(DeferredAction(() =>
                {
                    ShopManager.Instance?.SellItem(item.itemId, 1);
                    RefreshDisplay();
                }))
            );
        }
    }

    private void CreateItemCell(Sprite icon, string itemName, string description,
        int price, string actionLabel, bool canTransact, System.Action onClick)
    {
        if (itemContainer == null) return;

        var cellGO = new GameObject("ShopCell", typeof(RectTransform), typeof(Image));
        cellGO.transform.SetParent(itemContainer, false);

        var cellBg = cellGO.GetComponent<Image>();
        cellBg.color = canTransact ? CELL_BG : CELL_DISABLED;

        var cellOutline = cellGO.AddComponent<Outline>();
        cellOutline.effectColor = new Color(0.45f, 0.38f, 0.25f, 0.8f);
        cellOutline.effectDistance = new Vector2(2f, -2f);

        // Icon centered in top portion of cell (constrained size)
        var iconGO = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGO.transform.SetParent(cellGO.transform, false);
        var iconRT = iconGO.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.20f, 0.35f);
        iconRT.anchorMax = new Vector2(0.80f, 0.90f);
        iconRT.offsetMin = Vector2.zero;
        iconRT.offsetMax = Vector2.zero;

        var iconImg = iconGO.GetComponent<Image>();
        iconImg.raycastTarget = false;
        if (icon != null)
        {
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;
        }
        else
        {
            iconImg.color = new Color(0.3f, 0.3f, 0.3f, 0.2f);
        }

        // Price label — bottom 28%
        var priceObj = CreateTMPText(cellGO.transform, "Price", $"{price}G", 16,
            TextAlignmentOptions.Center, GOLD_COLOR);
        priceObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        priceObj.GetComponent<TextMeshProUGUI>().raycastTarget = false;
        var priceRT = priceObj.GetComponent<RectTransform>();
        priceRT.anchorMin = new Vector2(0, 0);
        priceRT.anchorMax = new Vector2(1, 0.28f);
        priceRT.offsetMin = Vector2.zero;
        priceRT.offsetMax = Vector2.zero;

        // Hover + click events
        var trigger = cellGO.AddComponent<EventTrigger>();

        AddTriggerEvent(trigger, EventTriggerType.PointerEnter, () =>
        {
            cellBg.color = CELL_HOVER;
            cellOutline.effectColor = GOLD_COLOR;
            ShowTooltip(cellGO.GetComponent<RectTransform>(), itemName, description, price);
        });

        AddTriggerEvent(trigger, EventTriggerType.PointerExit, () =>
        {
            cellBg.color = canTransact ? CELL_BG : CELL_DISABLED;
            cellOutline.effectColor = new Color(0.45f, 0.38f, 0.25f, 0.8f);
            HideTooltip();
        });

        AddTriggerEvent(trigger, EventTriggerType.PointerClick, () =>
        {
            onClick?.Invoke();
        });

        spawnedCells.Add(cellGO);
    }

    private void AddTriggerEvent(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((_) => action());
        trigger.triggers.Add(entry);
    }

    private void ShowTooltip(RectTransform cellRT, string itemName, string description,
        int price)
    {
        if (tooltipGO == null) return;

        tooltipName.text = itemName;
        tooltipDesc.text = !string.IsNullOrEmpty(description)
            ? description
            : LocalizationManager.LocalizeText("No description.");
        tooltipPrice.text = $"{price}G";

        // Position to the right of cell
        Vector3[] corners = new Vector3[4];
        cellRT.GetWorldCorners(corners);
        var parentRT = transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRT, RectTransformUtility.WorldToScreenPoint(null, corners[2]),
            null, out Vector2 localPos);

        tooltipRT.localPosition = new Vector3(localPos.x + 10, localPos.y, 0);
        ClampTooltip();

        tooltipCanvasGroup.alpha = 1;
    }

    private void HideTooltip()
    {
        if (tooltipCanvasGroup != null)
        {
            tooltipCanvasGroup.alpha = 0;
            tooltipCanvasGroup.blocksRaycasts = false;
            tooltipCanvasGroup.interactable = false;
        }
    }

    private void ClampTooltip()
    {
        if (tooltipRT == null) return;
        var panelRT = transform as RectTransform;
        if (panelRT == null) return;

        Vector3[] tc = new Vector3[4], pc = new Vector3[4];
        tooltipRT.GetWorldCorners(tc);
        panelRT.GetWorldCorners(pc);
        Vector3 pos = tooltipRT.localPosition;

        for (int i = 0; i < 4; i++)
        {
            tc[i] = panelRT.InverseTransformPoint(tc[i]);
            pc[i] = panelRT.InverseTransformPoint(pc[i]);
        }

        if (tc[2].x > pc[2].x) pos.x -= (tc[2].x - pc[2].x + 10);
        if (tc[0].x < pc[0].x) pos.x += (pc[0].x - tc[0].x + 10);
        if (tc[2].y > pc[2].y) pos.y -= (tc[2].y - pc[2].y + 10);
        if (tc[0].y < pc[0].y) pos.y += (pc[0].y - tc[0].y + 10);

        tooltipRT.localPosition = pos;
    }

    private GameObject CreateTMPText(Transform parent, string name, string defaultText,
        float fontSize, TextAlignmentOptions alignment, Color color)
    {
        var obj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        var tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        return obj;
    }

    /// <summary>
    /// Defers an action to the next frame to avoid Canvas rebuild conflicts.
    /// UI event handlers (PointerClick etc.) run during Canvas rebuild —
    /// modifying UI hierarchy or text in that context causes errors.
    /// </summary>
    private System.Collections.IEnumerator DeferredAction(System.Action action)
    {
        yield return null;
        action?.Invoke();
    }

    private void ClearCells()
    {
        foreach (var cell in spawnedCells)
            if (cell != null) Destroy(cell);
        spawnedCells.Clear();
    }
}
