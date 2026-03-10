using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shop UI panel that displays items from ShopCatalog.
/// Extends PanelBase for integration with UIManager.
///
/// Phase 2 Feature (#23): Shop UI panel.
///
/// Usage:
///   ShopPanel inherits from PanelBase — use UIManager.Instance.ShowPanel("shop") to open.
///   ShopManager.Instance.OpenShop(catalog) should be called before showing the panel.
/// </summary>
public class ShopPanel : PanelBase
{
    [Header("Shop UI References")]
    [Tooltip("Title text showing shop name.")]
    public TMP_Text shopTitle;

    [Tooltip("Text showing player's current Gold.")]
    public TMP_Text goldDisplay;

    [Tooltip("Container for shop item rows (should have VerticalLayoutGroup).")]
    public Transform itemContainer;

    [Tooltip("Prefab for a single shop item row.")]
    public GameObject shopItemPrefab;

    [Header("Tab Buttons")]
    [Tooltip("Button to show Buy tab.")]
    public Button buyTabButton;

    [Tooltip("Button to show Sell tab.")]
    public Button sellTabButton;

    [Tooltip("Close button.")]
    public Button closeButton;

    private enum ShopTab { Buy, Sell }
    private ShopTab currentTab = ShopTab.Buy;

    private List<GameObject> spawnedRows = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        panelId = "shop";
    }

    void Start()
    {
        if (buyTabButton != null)
            buyTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Buy));

        if (sellTabButton != null)
            sellTabButton.onClick.AddListener(() => SwitchTab(ShopTab.Sell));

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        // Listen for economy changes
        if (PlayerEconomyManager.Instance != null)
        {
            PlayerEconomyManager.Instance.OnGoldChanged += UpdateGoldDisplay;
        }
    }

    protected override void OnShow()
    {
        RefreshDisplay();
    }

    protected override void OnHide()
    {
        ShopManager.Instance?.CloseShop();
    }

    /// <summary>
    /// Switches between Buy and Sell tabs.
    /// </summary>
    private void SwitchTab(ShopTab tab)
    {
        currentTab = tab;
        RefreshDisplay();
    }

    /// <summary>
    /// Refreshes the entire shop display.
    /// </summary>
    public void RefreshDisplay()
    {
        // Update title
        if (shopTitle != null && ShopManager.Instance?.CurrentCatalog != null)
        {
            shopTitle.text = ShopManager.Instance.CurrentCatalog.shopName;
        }

        UpdateGoldDisplay(PlayerEconomyManager.Instance?.Gold ?? 0);
        ClearRows();

        if (currentTab == ShopTab.Buy)
        {
            PopulateBuyTab();
        }
        else
        {
            PopulateSellTab();
        }
    }

    /// <summary>
    /// Populates the Buy tab with items from the shop catalog.
    /// </summary>
    private void PopulateBuyTab()
    {
        if (ShopManager.Instance?.CurrentCatalog?.entries == null) return;

        foreach (var entry in ShopManager.Instance.CurrentCatalog.entries)
        {
            if (entry.item == null || !entry.available) continue;

            CreateItemRow(
                itemName: entry.item.itemName,
                description: entry.item.description,
                price: entry.buyPrice,
                priceLabel: LocalizationManager.LocalizeText("Buy"),
                canAfford: PlayerEconomyManager.Instance?.CanAffordGold(entry.buyPrice) ?? false,
                canTransact: entry.CanBuy,
                onClick: () =>
                {
                    ShopManager.Instance?.BuyItem(entry, 1);
                    RefreshDisplay();
                }
            );
        }
    }

    /// <summary>
    /// Populates the Sell tab with items from the player's inventory.
    /// </summary>
    private void PopulateSellTab()
    {
        var inventoryManager = ShopManager.Instance?.inventoryManager;
        if (inventoryManager == null) return;

        List<InvenItems> items = inventoryManager.GetInventoryItems();

        foreach (var item in items)
        {
            ShopEntry sellEntry = ShopManager.Instance.FindEntryByItemId(item.itemId);
            int sellPrice = sellEntry?.sellPrice ?? 0;

            if (sellPrice <= 0) continue;

            CreateItemRow(
                itemName: $"{item.name} x{item.quantity}",
                description: item.description,
                price: sellPrice,
                priceLabel: LocalizationManager.LocalizeText("Sell"),
                canAfford: true,
                canTransact: item.quantity > 0,
                onClick: () =>
                {
                    ShopManager.Instance?.SellItem(item.itemId, 1);
                    RefreshDisplay();
                }
            );
        }
    }

    /// <summary>
    /// Creates a single item row in the shop list.
    /// Falls back to a simple layout if no prefab is assigned.
    /// </summary>
    private void CreateItemRow(string itemName, string description, int price,
        string priceLabel, bool canAfford, bool canTransact, System.Action onClick)
    {
        if (itemContainer == null) return;

        GameObject row;

        if (shopItemPrefab != null)
        {
            row = Instantiate(shopItemPrefab, itemContainer);
        }
        else
        {
            // Fallback: create a simple row programmatically
            row = CreateSimpleRow(itemContainer);
        }

        // Try to find text components in the row
        TMP_Text[] texts = row.GetComponentsInChildren<TMP_Text>();
        if (texts.Length >= 1) texts[0].text = itemName;
        if (texts.Length >= 2) texts[1].text = $"{price}G";

        // Find or add button
        Button btn = row.GetComponentInChildren<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onClick());
            btn.interactable = canAfford && canTransact;

            TMP_Text btnText = btn.GetComponentInChildren<TMP_Text>();
            if (btnText != null) btnText.text = priceLabel;
        }

        spawnedRows.Add(row);
    }

    /// <summary>
    /// Creates a simple row without a prefab (fallback).
    /// </summary>
    private GameObject CreateSimpleRow(Transform parent)
    {
        GameObject row = new GameObject("ShopItemRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);

        var rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 40);

        var hlg = row.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;

        // Item name text
        CreateTMPText(row.transform, "ItemName", LocalizationManager.LocalizeText("Item"));

        // Price text
        CreateTMPText(row.transform, "PriceText", "0G");

        // Button
        GameObject btnObj = new GameObject("ActionButton", typeof(RectTransform), typeof(Button), typeof(Image));
        btnObj.transform.SetParent(row.transform, false);
        btnObj.GetComponent<Image>().color = new Color(0.3f, 0.6f, 0.3f, 1f);
        CreateTMPText(btnObj.transform, "BtnText", LocalizationManager.LocalizeText("Buy"));

        return row;
    }

    private void CreateTMPText(Transform parent, string name, string defaultText)
    {
        GameObject textObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(parent, false);
        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.color = Color.white;
    }

    private void ClearRows()
    {
        foreach (var row in spawnedRows)
        {
            if (row != null) Destroy(row);
        }
        spawnedRows.Clear();
    }

    private void UpdateGoldDisplay(int gold)
    {
        if (goldDisplay != null)
        {
            goldDisplay.text = LocalizationManager.LocalizeText($"Gold: {gold}");
        }
    }

    void OnDestroy()
    {
        if (PlayerEconomyManager.Instance != null)
        {
            PlayerEconomyManager.Instance.OnGoldChanged -= UpdateGoldDisplay;
        }
    }
}
