using PolyAndCode.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Inventory cell that displays item info and handles click/hover interactions.
/// Phase 2 Enhancement (#28): Added click-to-select, tooltip on hover, quantity display.
/// Phase 3: Added icon loading from ItemDefinition assets or sprite resources.
/// </summary>
public class CelllItemData : MonoBehaviour, ICell, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public Text nameLabel;
    public Text desLabel;
    public TMP_Text quantityLabel;
    public Image iconImage;
    public Image highlightImage;

    // Model
    private InvenItems _contactInfo;
    private int _cellIndex;
    private Image _quickbarBadgeImage;
    private Text _quickbarBadgeLabel;
    private bool _isSelected;
    private bool _styleApplied;

    // Sprite cache to avoid repeated lookups
    private static readonly System.Collections.Generic.Dictionary<string, Sprite> _spriteCache
        = new System.Collections.Generic.Dictionary<string, Sprite>();

    /// <summary>
    /// Called from the SetCell method in DataSource.
    /// Configures display for the given inventory item.
    /// </summary>
    public void ConfigureCell(InvenItems invenItems, int cellIndex)
    {
        EnsureGridStyle();

        _cellIndex = cellIndex;
        _contactInfo = invenItems;

        if (nameLabel != null)
        {
            nameLabel.text = GetShortDisplayName(invenItems.name);
        }

        if (desLabel != null)
        {
            desLabel.text = invenItems.description;
            desLabel.gameObject.SetActive(false);
        }

        // Show quantity badge for stacked items.
        if (quantityLabel != null)
        {
            quantityLabel.text = invenItems.quantity > 1 ? invenItems.quantity.ToString() : "";
        }

        // Load item icon
        if (iconImage != null)
        {
            Sprite sprite = FindItemSprite(invenItems);
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }

        UpdateQuickbarBadge();

        // Deselect visual on reconfigure
        _isSelected = false;
        SetHighlight(false);
    }

    /// <summary>
    /// Finds the best matching sprite for an inventory item.
    /// Lookup order: cache → ItemDefinition asset → iconName resource → itemId resource.
    /// </summary>
    private static Sprite FindItemSprite(InvenItems item)
    {
        if (item == null) return null;

        string cacheKey = item.itemId ?? item.name;
        if (_spriteCache.TryGetValue(cacheKey, out Sprite cached))
            return cached;

        Sprite result = null;

        // 1. Try ItemDefinition asset (has icon field)
        var definitions = Resources.FindObjectsOfTypeAll<ItemDefinition>();
        foreach (var def in definitions)
        {
            if (def.itemId == item.itemId && def.icon != null)
            {
                result = def.icon;
                break;
            }
        }

        // 2. Try loading by iconName from Resources
        if (result == null && !string.IsNullOrEmpty(item.iconName))
        {
            result = Resources.Load<Sprite>($"Sprites/{item.iconName}");
            if (result == null)
                result = Resources.Load<Sprite>(item.iconName);
        }

        // 3. Try loading by itemId from Resources
        if (result == null && !string.IsNullOrEmpty(item.itemId))
        {
            result = Resources.Load<Sprite>($"Sprites/{item.itemId}");
            if (result == null)
                result = Resources.Load<Sprite>(item.itemId);
        }

        // 4. Search all loaded sprites by name match
        if (result == null)
        {
            var allSprites = Resources.FindObjectsOfTypeAll<Sprite>();
            string target = item.itemId ?? item.iconName ?? "";
            foreach (var s in allSprites)
            {
                if (s.name == target)
                {
                    result = s;
                    break;
                }
            }
        }

        _spriteCache[cacheKey] = result;
        return result;
    }

    /// <summary>
    /// Handles click — opens the inventory action panel for this item.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_contactInfo == null) return;

        if (eventData.button == PointerEventData.InputButton.Right && IsSeed(_contactInfo))
        {
            ToggleQuickbarAssignment();
            return;
        }

        // Show action panel
        if (InventoryActionPanel.Instance != null)
        {
            InventoryActionPanel.Instance.Show(_contactInfo, _cellIndex);
        }

        // Show tooltip at click position
        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.Show(_contactInfo, eventData.position);
        }

        _isSelected = true;
        SetHighlight(true);
        AudioManager.Instance?.PlaySFX("ui_click");
    }

    /// <summary>
    /// Handles pointer enter — shows tooltip.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_contactInfo == null) return;

        SetHighlight(true);

        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.Show(_contactInfo, eventData.position);
        }
    }

    /// <summary>
    /// Handles pointer exit — hides tooltip.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isSelected)
        {
            SetHighlight(false);
        }

        if (ItemTooltip.Instance != null)
        {
            ItemTooltip.Instance.Hide();
        }
    }

    private void SetHighlight(bool active)
    {
        if (highlightImage != null)
        {
            highlightImage.enabled = active;
        }
    }

    private void EnsureGridStyle()
    {
        if (_styleApplied) return;
        _styleApplied = true;

        RectTransform root = transform as RectTransform;
        if (root != null)
        {
            root.sizeDelta = new Vector2(
                Mathf.Max(root.sizeDelta.x, 84f),
                Mathf.Max(root.sizeDelta.y, 84f)
            );
        }

        Image background = GetComponent<Image>();
        if (background != null)
        {
            background.color = new Color(0.56f, 0.42f, 0.25f, 0.96f);
        }

        Outline outline = GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
        }
        outline.effectColor = new Color(0.24f, 0.16f, 0.08f, 0.7f);
        outline.effectDistance = new Vector2(2f, -2f);

        if (iconImage != null)
        {
            RectTransform iconRT = iconImage.rectTransform;
            iconRT.anchorMin = new Vector2(0.16f, 0.24f);
            iconRT.anchorMax = new Vector2(0.84f, 0.86f);
            iconRT.offsetMin = Vector2.zero;
            iconRT.offsetMax = Vector2.zero;
            iconImage.preserveAspect = true;
        }

        if (nameLabel != null)
        {
            RectTransform nameRT = nameLabel.rectTransform;
            nameRT.anchorMin = new Vector2(0.06f, 0.02f);
            nameRT.anchorMax = new Vector2(0.94f, 0.22f);
            nameRT.offsetMin = Vector2.zero;
            nameRT.offsetMax = Vector2.zero;
            nameLabel.alignment = TextAnchor.MiddleCenter;
            nameLabel.fontSize = 10;
            nameLabel.color = new Color(0.97f, 0.92f, 0.78f, 1f);
        }

        if (desLabel != null)
        {
            desLabel.gameObject.SetActive(false);
        }

        if (quantityLabel != null)
        {
            RectTransform qtyRT = quantityLabel.rectTransform;
            qtyRT.anchorMin = new Vector2(0.62f, 0.7f);
            qtyRT.anchorMax = new Vector2(0.96f, 0.96f);
            qtyRT.offsetMin = Vector2.zero;
            qtyRT.offsetMax = Vector2.zero;
            quantityLabel.alignment = TextAlignmentOptions.TopRight;
            quantityLabel.fontSize = 16;
            quantityLabel.color = new Color(1f, 0.9f, 0.3f, 1f);
        }

        EnsureQuickbarBadge();
    }

    private void EnsureQuickbarBadge()
    {
        Transform badgeTransform = transform.Find("QuickbarBadge");
        if (badgeTransform == null)
        {
            GameObject badgeGO = new GameObject("QuickbarBadge", typeof(RectTransform), typeof(Image));
            badgeGO.transform.SetParent(transform, false);

            RectTransform badgeRT = badgeGO.GetComponent<RectTransform>();
            badgeRT.anchorMin = new Vector2(0.03f, 0.74f);
            badgeRT.anchorMax = new Vector2(0.3f, 0.97f);
            badgeRT.offsetMin = Vector2.zero;
            badgeRT.offsetMax = Vector2.zero;

            _quickbarBadgeImage = badgeGO.GetComponent<Image>();
            _quickbarBadgeImage.color = new Color(0.14f, 0.45f, 0.2f, 0.95f);

            GameObject labelGO = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGO.transform.SetParent(badgeGO.transform, false);
            RectTransform labelRT = labelGO.GetComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.offsetMin = Vector2.zero;
            labelRT.offsetMax = Vector2.zero;

            _quickbarBadgeLabel = labelGO.GetComponent<Text>();
            _quickbarBadgeLabel.alignment = TextAnchor.MiddleCenter;
            _quickbarBadgeLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            _quickbarBadgeLabel.fontSize = 12;
            _quickbarBadgeLabel.fontStyle = FontStyle.Bold;
            _quickbarBadgeLabel.color = Color.white;
        }
        else
        {
            _quickbarBadgeImage = badgeTransform.GetComponent<Image>();
            _quickbarBadgeLabel = badgeTransform.GetComponentInChildren<Text>(true);
        }

        if (_quickbarBadgeImage != null)
        {
            _quickbarBadgeImage.enabled = false;
        }

        if (_quickbarBadgeLabel != null)
        {
            _quickbarBadgeLabel.gameObject.SetActive(false);
        }
    }

    private void UpdateQuickbarBadge()
    {
        EnsureQuickbarBadge();

        int assignedIndex = SeedQuickbarUI.Instance != null && _contactInfo != null
            ? SeedQuickbarUI.Instance.GetAssignedSlotIndex(_contactInfo.itemId)
            : -1;

        bool isAssigned = assignedIndex >= 0;
        if (_quickbarBadgeImage != null) _quickbarBadgeImage.enabled = isAssigned;
        if (_quickbarBadgeLabel != null)
        {
            _quickbarBadgeLabel.gameObject.SetActive(isAssigned);
            _quickbarBadgeLabel.text = isAssigned ? (assignedIndex + 1).ToString() : "";
        }
    }

    private void ToggleQuickbarAssignment()
    {
        var quickbar = SeedQuickbarUI.Instance ?? FindObjectOfType<SeedQuickbarUI>();
        if (quickbar == null)
        {
            NotificationManager.Instance?.ShowNotification(
                LocalizationManager.LocalizeText("Quickbar not found."), 1.5f);
            return;
        }

        bool isAssigned = quickbar.IsSeedInQuickbar(_contactInfo.itemId);
        if (isAssigned)
        {
            quickbar.RemoveFromQuickbar(_contactInfo.itemId);
            NotificationManager.Instance?.ShowNotification(
                LocalizationManager.LocalizeText($"Removed {_contactInfo.name} from Quickbar."), 1.5f);
        }
        else
        {
            bool added = quickbar.AssignToQuickbar(_contactInfo.itemId);
            NotificationManager.Instance?.ShowNotification(
                LocalizationManager.LocalizeText(
                    added
                        ? $"Assigned {_contactInfo.name} to Quickbar!"
                        : "Quickbar is full (max 9 slots)."
                ),
                1.5f
            );
        }

        AudioManager.Instance?.PlaySFX("ui_click");
        UpdateQuickbarBadge();
        FindInventoryManager()?.RefreshUI();
    }

    private static bool IsSeed(InvenItems item)
    {
        if (item == null) return false;
        return string.Equals(item.itemType, "seed", System.StringComparison.OrdinalIgnoreCase)
            || (!string.IsNullOrEmpty(item.itemId) && item.itemId.StartsWith("seed_"));
    }

    private static string GetShortDisplayName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "";

        string value = fullName.Trim();
        if (value.StartsWith("Hạt ")) return value.Substring(4);
        if (value.StartsWith("Seed ")) return value.Substring(5);
        return value;
    }

    private static RecyclableInventoryManager FindInventoryManager()
    {
        GameObject inventoryGO = GameObject.Find("InventoryManager");
        return inventoryGO != null ? inventoryGO.GetComponent<RecyclableInventoryManager>() : null;
    }
}
