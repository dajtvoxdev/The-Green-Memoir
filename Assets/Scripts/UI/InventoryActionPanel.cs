using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Context action panel for inventory items.
/// Shows Use/Equip/Drop/Split buttons when an inventory item is selected.
///
/// Phase 2 Feature (#28): Inventory context actions.
///
/// Usage: Attach to a panel under Canvas. Shown via InventoryActionPanel.Instance.Show(item, index).
/// </summary>
public class InventoryActionPanel : MonoBehaviour
{
    public static InventoryActionPanel Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text selectedItemLabel;
    public Button useButton;
    public Button equipButton;
    public Button dropButton;
    public Button splitButton;
    public Button closeButton;

    [Header("Button Texts (auto-found if null)")]
    public TMP_Text useButtonText;
    public TMP_Text equipButtonText;

    private CanvasGroup canvasGroup;
    private InvenItems selectedItem;
    private int selectedIndex = -1;

    /// <summary>
    /// Fired when an action modifies inventory. Listeners should refresh UI.
    /// </summary>
    public event Action OnInventoryAction;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        SetupButtons();
        Hide();
    }

    private void SetupButtons()
    {
        if (useButton != null) useButton.onClick.AddListener(OnUseClicked);
        if (equipButton != null) equipButton.onClick.AddListener(OnEquipClicked);
        if (dropButton != null) dropButton.onClick.AddListener(OnDropClicked);
        if (splitButton != null) splitButton.onClick.AddListener(OnSplitClicked);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
    }

    /// <summary>
    /// Shows the action panel for a selected inventory item.
    /// </summary>
    public void Show(InvenItems item, int index)
    {
        if (item == null) return;

        selectedItem = item;
        selectedIndex = index;

        if (selectedItemLabel != null)
        {
            selectedItemLabel.text = $"{item.name} x{item.quantity}";
        }

        // Configure button visibility based on item type
        ConfigureButtons(item);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    /// <summary>
    /// Hides the action panel.
    /// </summary>
    public void Hide()
    {
        selectedItem = null;
        selectedIndex = -1;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    /// <summary>
    /// Configures which buttons are visible based on item type.
    /// </summary>
    private void ConfigureButtons(InvenItems item)
    {
        string type = item.itemType?.ToLower() ?? "";

        // Use button: consumables, seeds
        bool canUse = type == "consumable" || type == "seed";
        if (useButton != null) useButton.gameObject.SetActive(canUse);
        if (canUse && useButtonText != null)
        {
            useButtonText.text = type == "seed"
                ? LocalizationManager.LocalizeText("Plant")
                : LocalizationManager.LocalizeText("Use");
        }

        // Equip button: tools
        bool canEquip = type == "tool";
        if (equipButton != null) equipButton.gameObject.SetActive(canEquip);

        // Drop button: always available
        if (dropButton != null) dropButton.gameObject.SetActive(true);

        // Split button: only for stacks > 1
        bool canSplit = item.quantity > 1;
        if (splitButton != null) splitButton.gameObject.SetActive(canSplit);
    }

    /// <summary>
    /// Use action: consume item or equip seed for planting.
    /// Phase 2.5B Fix (BP1): Seeds now pass their specific cropId to EquipmentManager.
    /// </summary>
    private void OnUseClicked()
    {
        if (selectedItem == null) return;

        string type = selectedItem.itemType?.ToLower() ?? "";

        if (type == "seed")
        {
            EquipSeedFromInventory(selectedItem);
        }
        else if (type == "consumable")
        {
            RemoveQuantity(1);
            NotificationManager.Instance?.ShowNotification(
                LocalizationManager.LocalizeText($"Used {selectedItem.name}."), 1.5f);
            AudioManager.Instance?.PlaySFX("ui_click");
        }

        OnInventoryAction?.Invoke();
        Hide();
    }

    /// <summary>
    /// Equips a seed item for planting by finding or creating a matching SeedBag tool.
    /// Resolves BP1: Inventory seed → EquipmentManager with correct cropId.
    /// </summary>
    private void EquipSeedFromInventory(InvenItems seedItem)
    {
        if (EquipmentManager.Instance == null) return;

        // First try to find an existing SeedBag tool that matches this seed's cropId
        string targetCropId = seedItem.itemId; // Seeds use itemId as cropId link
        ToolDefinition matchingTool = null;

        if (EquipmentManager.Instance.starterTools != null)
        {
            foreach (var tool in EquipmentManager.Instance.starterTools)
            {
                if (tool != null && tool.toolType == ToolType.SeedBag && tool.cropId == targetCropId)
                {
                    matchingTool = tool;
                    break;
                }
            }
        }

        if (matchingTool != null)
        {
            EquipmentManager.Instance.EquipTool(matchingTool);
        }
        else
        {
            // Create a runtime SeedBag tool for this specific seed
            var runtimeTool = ScriptableObject.CreateInstance<ToolDefinition>();
            runtimeTool.toolId = $"seedbag_{seedItem.itemId}";
            runtimeTool.toolName = $"{seedItem.name} Bag";
            runtimeTool.description = $"Plant {seedItem.name}";
            runtimeTool.toolType = ToolType.SeedBag;
            runtimeTool.cropId = targetCropId;
            runtimeTool.tier = 1;

            EquipmentManager.Instance.EquipTool(runtimeTool);
        }

        NotificationManager.Instance?.ShowNotification(
            LocalizationManager.LocalizeText($"Ready to plant {seedItem.name}! Press E or V to plant."), 2f);
        AudioManager.Instance?.PlaySFX("equip");
    }

    /// <summary>
    /// Equip action: equip tool item.
    /// </summary>
    private void OnEquipClicked()
    {
        if (selectedItem == null) return;

        // Find matching ToolDefinition by itemId or name
        if (EquipmentManager.Instance != null && EquipmentManager.Instance.starterTools != null)
        {
            foreach (var tool in EquipmentManager.Instance.starterTools)
            {
                if (tool != null && tool.toolId == selectedItem.itemId)
                {
                    EquipmentManager.Instance.EquipTool(tool);
                    break;
                }
            }
        }

        OnInventoryAction?.Invoke();
        Hide();
    }

    /// <summary>
    /// Drop action: remove one item from inventory.
    /// </summary>
    private void OnDropClicked()
    {
        if (selectedItem == null || selectedIndex < 0) return;

        RemoveQuantity(1);

        NotificationManager.Instance?.ShowNotification(
            LocalizationManager.LocalizeText($"Dropped 1x {selectedItem.name}."), 1.5f);
        AudioManager.Instance?.PlaySFX("ui_click");
        OnInventoryAction?.Invoke();
        Hide();
    }

    /// <summary>
    /// Split action: splits the stack in half.
    /// </summary>
    private void OnSplitClicked()
    {
        if (selectedItem == null || selectedIndex < 0 || selectedItem.quantity <= 1) return;

        RecyclableInventoryManager invManager = FindInventoryManager();
        if (invManager == null) return;

        int splitAmount = selectedItem.quantity / 2;

        // Create new stack with half the quantity
        InvenItems newStack = new InvenItems(
            itemId: selectedItem.itemId,
            name: selectedItem.name,
            description: selectedItem.description,
            quantity: splitAmount,
            itemType: selectedItem.itemType,
            iconName: selectedItem.iconName
        );

        // Remove split amount from original
        invManager.RemoveQuantityAt(selectedIndex, splitAmount);

        // Add new stack
        invManager.AddInventoryItem(newStack);

        NotificationManager.Instance?.ShowNotification(
            LocalizationManager.LocalizeText($"Split {selectedItem.name} ({splitAmount})"), 1.5f);
        AudioManager.Instance?.PlaySFX("ui_click");
        OnInventoryAction?.Invoke();
        Hide();
    }

    /// <summary>
    /// Removes quantity from the selected item in inventory.
    /// </summary>
    private void RemoveQuantity(int amount)
    {
        RecyclableInventoryManager invManager = FindInventoryManager();
        if (invManager != null && selectedIndex >= 0)
        {
            invManager.RemoveQuantityAt(selectedIndex, amount);
        }
    }

    private RecyclableInventoryManager FindInventoryManager()
    {
        GameObject invGO = GameObject.Find("InventoryManager");
        return invGO?.GetComponent<RecyclableInventoryManager>();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
