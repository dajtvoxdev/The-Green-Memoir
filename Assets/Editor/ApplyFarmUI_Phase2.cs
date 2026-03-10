using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ApplyFarmUI_Phase2
{
    [MenuItem("Tools/ApplyFarmUI_Phase2")]
    public static void Apply()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        // 1. Hide TopBar background completely as requested (Thanh header màu trắng khó nhìn)
        Transform hudTopBar = canvas.transform.Find("HUDTopBarBG");
        if (hudTopBar != null)
        {
            Image bgImage = hudTopBar.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.enabled = false; // Completely hide the background
            }
        }

        // 2. Hide User Name Wizard
        Transform nameWizard = canvas.transform.Find("UI Name Wizard");
        if (nameWizard != null)
        {
            nameWizard.gameObject.SetActive(false);
            Debug.Log("Disabled UI Name Wizard.");
        }
        
        // Hide any Username text under HUDTopBarBG just in case
        if (hudTopBar != null) {
            Transform userName = hudTopBar.Find("UI Information In Game");
            if (userName != null) userName.gameObject.SetActive(false);
        }

        // Load Sprites for Icons
        Sprite goldSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny RPG Forest/Artwork/sprites/misc/coin/coin-1.png");
        Sprite gemSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Tiny RPG Forest/Artwork/sprites/misc/gem/gem-1.png");
        Sprite sunSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Rustic UI/UI-Singles/UI - 15.png"); // Using a round UI element as a placeholder for sun if needed, or we can use coin.
        if(sunSprite == null) sunSprite = goldSprite; // Fallback

        // 3. Setup Economy (Gold, Diamond)
        // Find existing Gold Text
        Text goldTextLegacy = null;
        TextMeshProUGUI goldTextTMP = null;
        Transform goldTransform = null;
        
        foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Contains("Gold"))
            {
                goldTransform = t;
                goldTextLegacy = t.GetComponent<Text>();
                goldTextTMP = t.GetComponent<TextMeshProUGUI>();
                if(goldTextLegacy != null || goldTextTMP != null) break;
            }
        }

        Transform diamondTransform = null;
        foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Contains("Diamond") || t.name.Contains("Gem"))
            {
                diamondTransform = t;
                if(t.GetComponent<Text>() != null || t.GetComponent<TextMeshProUGUI>() != null) break;
            }
        }

        // 4. Create Icons and organize
        if (goldTransform != null)
        {
            // Remove text "Gold:" or just leave as is, but let's add an icon.
            CreateIconNextTo(goldTransform, goldSprite, "GoldIcon", new Vector2(-40, 0));
        }

        if (diamondTransform != null)
        {
            CreateIconNextTo(diamondTransform, gemSprite, "DiamondIcon", new Vector2(-40, 0));
        }

        // 5. Setup Time/Day Icon
        Transform timeTransform = null;
        foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (t.name.Contains("Time") || t.name.Contains("Day"))
            {
                timeTransform = t;
                if(t.GetComponent<Text>() != null || t.GetComponent<TextMeshProUGUI>() != null) break;
            }
        }

        if (timeTransform != null && !timeTransform.name.Contains("Icon"))
        {
            CreateIconNextTo(timeTransform, sunSprite, "DayNightIcon", new Vector2(-40, 0));
        }
        
        // Ensure regular UI Texts also use Cherry Bomb
        Font ttfFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular.ttf");
        if (ttfFont != null)
        {
            Text[] legacyTexts = canvas.GetComponentsInChildren<Text>(true);
            foreach(var txt in legacyTexts)
            {
                txt.font = ttfFont;
                txt.color = new Color32(250, 240, 220, 255);
            }
        }

        Debug.Log("ApplyFarmUI_Phase2 Completed.");
    }

    private static void CreateIconNextTo(Transform textTransform, Sprite iconSprite, string iconName, Vector2 offset)
    {
        if (iconSprite == null) return;
        
        // Check if already exists
        Transform existing = textTransform.parent.Find(iconName);
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        GameObject iconObj = new GameObject(iconName);
        iconObj.transform.SetParent(textTransform.parent, false);
        
        Image img = iconObj.AddComponent<Image>();
        img.sprite = iconSprite;
        img.preserveAspect = true;

        RectTransform iconRt = iconObj.GetComponent<RectTransform>();
        RectTransform txtRt = textTransform.GetComponent<RectTransform>();
        
        // Position icon to the left of the text
        iconRt.anchorMin = txtRt.anchorMin;
        iconRt.anchorMax = txtRt.anchorMax;
        iconRt.pivot = txtRt.pivot;
        iconRt.sizeDelta = new Vector2(40, 40); // Standard icon size
        iconRt.anchoredPosition = txtRt.anchoredPosition + offset;

        // Optionally add a subtle shadow
        Shadow shadow = iconObj.AddComponent<Shadow>();
        shadow.effectColor = new Color(0,0,0,0.5f);
        shadow.effectDistance = new Vector2(2, -2);
    }
}
