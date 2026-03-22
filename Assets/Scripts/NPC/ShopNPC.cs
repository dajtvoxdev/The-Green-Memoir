using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// NPC that opens a shop when the player interacts (E key) within trigger range.
/// Place on a GameObject with a BoxCollider2D (isTrigger = true).
///
/// Phase 2.5A Fix (BP2): Provides gameplay access to ShopManager.OpenShop().
///
/// Setup:
///   1. Create GameObject with SpriteRenderer (shop keeper sprite)
///   2. Add BoxCollider2D, set isTrigger = true, size ~(2, 2)
///   3. Attach this script, assign shopCatalog in Inspector
///   4. Player must have a Collider2D + Rigidbody2D
/// </summary>
public class ShopNPC : MonoBehaviour
{
    [Header("Shop Settings")]
    [Tooltip("The ShopCatalog this NPC sells. Create via MoonlitGarden > Shop > ShopCatalog.")]
    public ShopCatalog shopCatalog;

    [Header("Interaction")]
    [Tooltip("Key to interact with the NPC.")]
    public KeyCode interactKey = KeyCode.E;

    [Tooltip("Prompt text shown when player is in range.")]
    public string interactPrompt = "Press E to open shop";
    
    [Tooltip("Vietnamese prompt text.")]
    public string interactPromptVN = "Nhấn E để mở cửa hàng";

    [Header("UI References (auto-found if null)")]
    [Tooltip("Optional: reference to prompt UI text. Auto-creates if null.")]
    public GameObject promptUI;

    private bool playerInRange;
    private GameObject autoPromptUI;
    private bool isVietnamese = true; // Default to Vietnamese

    void Start()
    {
        // Default to Vietnamese, check PlayerPrefs for user override
        string lang = PlayerPrefs.GetString("GameLanguage", "vi");
        isVietnamese = (lang == "vi");
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey) && !IsShopAlreadyOpen())
        {
            OpenShop();
        }
    }

    private void OpenShop()
    {
        if (shopCatalog == null)
        {
            Debug.LogWarning("ShopNPC: No ShopCatalog assigned!");
            NotificationManager.Instance?.ShowNotification("Cửa hàng chưa sẵn sàng.");
            return;
        }

        ShopManager.Instance?.OpenShop(shopCatalog);

        // Try to show shop panel if it exists, otherwise show notification
        if (UIManager.Instance != null && UIManager.Instance.GetPanel("shop") != null)
        {
            UIManager.Instance.ShowPanel("shop");
        }
        else
        {
            NotificationManager.Instance?.ShowNotification("Cửa hàng đang được xây dựng...");
            Debug.Log("ShopNPC: Shop panel not yet created in UIManager. ShopManager.OpenShop() was called successfully.");
        }

        AudioManager.Instance?.PlaySFX("ui_click");
        HidePrompt();
    }

    private bool IsShopAlreadyOpen()
    {
        return UIManager.Instance != null && UIManager.Instance.IsPanelOpen("shop");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        HidePrompt();
    }

    private void ShowPrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
            return;
        }

        // Auto-create a screen-space prompt that follows the NPC
        if (autoPromptUI == null)
        {
            autoPromptUI = CreateScreenPrompt();
        }
        autoPromptUI.SetActive(true);
    }

    private void HidePrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
            return;
        }

        if (autoPromptUI != null)
        {
            autoPromptUI.SetActive(false);
        }
    }

    /// <summary>
    /// Creates a screen-space prompt at the bottom-right corner of the screen.
    /// Uses the existing Canvas in the scene for consistent UI rendering.
    /// </summary>
    private GameObject CreateScreenPrompt()
    {
        // Find existing screen-space Canvas
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            // Fallback: find any canvas
            foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            {
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    canvas = c;
                    break;
                }
            }
        }
        if (canvas == null) return new GameObject("ShopPrompt"); // safety fallback

        // Create prompt container anchored to bottom-right
        var go = new GameObject("ShopPrompt");
        go.transform.SetParent(canvas.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0); // bottom-right
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-20f, 20f); // 20px margin from corner
        rt.sizeDelta = new Vector2(260f, 60f);

        // Background panel
        var bgImage = go.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.75f);

        // Key hint [E] on the left
        var keyHintGO = new GameObject("KeyHint");
        keyHintGO.transform.SetParent(go.transform, false);
        var keyHint = keyHintGO.AddComponent<TMPro.TextMeshProUGUI>();
        keyHint.text = "[E]";
        keyHint.fontSize = 22;
        keyHint.fontStyle = TMPro.FontStyles.Bold;
        keyHint.alignment = TMPro.TextAlignmentOptions.Center;
        keyHint.color = new Color(1f, 0.8f, 0f); // Gold

        var keyRT = keyHintGO.GetComponent<RectTransform>();
        keyRT.anchorMin = new Vector2(0, 0);
        keyRT.anchorMax = new Vector2(0, 1);
        keyRT.pivot = new Vector2(0, 0.5f);
        keyRT.anchoredPosition = new Vector2(8f, 0);
        keyRT.sizeDelta = new Vector2(40f, 0);

        // Main text
        var textGO = new GameObject("PromptText");
        textGO.transform.SetParent(go.transform, false);
        var tmp = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = isVietnamese ? interactPromptVN : interactPrompt;
        tmp.fontSize = 18;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;

        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 0);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.offsetMin = new Vector2(45f, 5f);  // Left padding for [E]
        textRT.offsetMax = new Vector2(-5f, -5f);

        return go;
    }

    void OnDestroy()
    {
        if (autoPromptUI != null)
        {
            Destroy(autoPromptUI);
        }
    }
}