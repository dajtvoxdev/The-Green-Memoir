using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ApplyFarmUI_Phase3
{
    [MenuItem("Tools/ApplyFarmUI_Phase3")]
    public static void Apply()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        // 1. Hide HUD backgrounds (Gold/Diamond box and Time box)
        foreach (var img in canvas.GetComponentsInChildren<Image>(true))
        {
            if (img.gameObject.name.Contains("EconomyHUD") || img.gameObject.name.Contains("TimeHUD"))
            {
                img.enabled = false;
            }
        }

        // 2. Setup Economy UI to be horizontal and remove label texts
        Transform economyHud = null;
        Transform timeHud = null;

        foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "EconomyHUD") economyHud = t;
            if (t.name == "TimeHUD") timeHud = t;
        }

        if (economyHud != null)
        {
            // Usually EconomyHUD has a VerticalLayoutGroup. Let's change it to HorizontalLayoutGroup
            VerticalLayoutGroup vlg = economyHud.GetComponent<VerticalLayoutGroup>();
            if (vlg != null)
            {
                Object.DestroyImmediate(vlg);
                HorizontalLayoutGroup hlg = economyHud.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleRight;
                hlg.childControlHeight = true;
                hlg.childControlWidth = true;
                hlg.spacing = 20;

                RectTransform rect = economyHud.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(400, 60); // wider
                rect.anchoredPosition = new Vector2(-20, -40); // align right
                rect.pivot = new Vector2(1, 1);
                rect.anchorMin = new Vector2(1, 1);
                rect.anchorMax = new Vector2(1, 1);
            }

            // Clean up Gold Text
            Transform goldObj = economyHud.Find("Gold");
            if (goldObj != null)
            {
                // To keep the script reference working, we only change the display
                // But we don't know the exact script. Let's assume it updates text to "Gold: 100".
                // We shouldn't rename or destroy the object if it's referenced.
                // However, we can use a HorizontalLayoutGroup for it
                SetupHorizontalVal(goldObj, "Assets/Tiny RPG Forest/Artwork/sprites/misc/coin/coin-1.png", "GoldIcon");
            }

            // Clean up Diamond Text
            Transform diamondObj = economyHud.Find("Diamond");
            if (diamondObj != null)
            {
                SetupHorizontalVal(diamondObj, "Assets/Tiny RPG Forest/Artwork/sprites/misc/gem/gem-1.png", "DiamondIcon");
            }
        }

        // 3. Setup Time UI to be horizontal
        if (timeHud != null)
        {
            // Time HUD currently has TimeText and maybe DayText
            HorizontalLayoutGroup hlg = timeHud.GetComponent<HorizontalLayoutGroup>();
            if (hlg == null)
            {
                hlg = timeHud.gameObject.AddComponent<HorizontalLayoutGroup>();
                hlg.childAlignment = TextAnchor.MiddleCenter;
                hlg.spacing = 10;
                
                RectTransform rect = timeHud.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(300, 60); 
            }

            // Remove previous icons added in phase 2 that were manually positioned
            Transform oldIcon = timeHud.parent.Find("DayNightIcon");
            if (oldIcon != null) Object.DestroyImmediate(oldIcon.gameObject);

            // Add new layout icon
            Transform dayNightIcon = timeHud.Find("DayNightIcon");
            if (dayNightIcon == null)
            {
                GameObject iconObj = new GameObject("DayNightIcon");
                iconObj.transform.SetParent(timeHud, false);
                iconObj.transform.SetAsFirstSibling(); // left side
                
                Image img = iconObj.AddComponent<Image>();
                Sprite sunSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Rustic UI/UI-Singles/UI - 15.png");
                img.sprite = sunSprite;
                img.preserveAspect = true;

                LayoutElement le = iconObj.AddComponent<LayoutElement>();
                le.minWidth = 40;
                le.minHeight = 40;
                le.preferredWidth = 40;
                le.preferredHeight = 40;
            }
        }

        Debug.Log("ApplyFarmUI_Phase3 (Layout Fixed) Completed.");
    }

    private static void SetupHorizontalVal(Transform textNode, string spritePath, string iconName)
    {
        // For textNode, we will create a parent Horizontal layout to hold Icon + Text,
        // but since scripts might update the text directly, we shouldn't change its hierarchy.
        // Actually, easiest way is to add an Icon inside textNode and structure it.
        // Or simply add a HorizontalLayoutGroup to textNode (which has Text component).
        
        HorizontalLayoutGroup hlg = textNode.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null)
        {
            hlg = textNode.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.spacing = 10;
        }
        
        // Remove text content alignment issue
        Text legacyTxt = textNode.GetComponent<Text>();
        if(legacyTxt) legacyTxt.alignment = TextAnchor.MiddleLeft;
        
        TextMeshProUGUI tmpTxt = textNode.GetComponent<TextMeshProUGUI>();
        if(tmpTxt) tmpTxt.alignment = TextAlignmentOptions.Left;

        // Clean up old manual icon from Phase 2
        Transform oldIcon = textNode.parent.Find(iconName);
        if (oldIcon != null) Object.DestroyImmediate(oldIcon.gameObject);

        // Add Icon as child
        Transform iconTransform = textNode.Find(iconName);
        if (iconTransform == null)
        {
            GameObject iconObj = new GameObject(iconName);
            iconObj.transform.SetParent(textNode, false);
            iconObj.transform.SetAsFirstSibling();
            
            Image img = iconObj.AddComponent<Image>();
            Sprite spr = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            img.sprite = spr;
            img.preserveAspect = true;

            LayoutElement le = iconObj.AddComponent<LayoutElement>();
            le.minWidth = 32;
            le.minHeight = 32;
            le.preferredWidth = 32;
            le.preferredHeight = 32;
        }

        // To fix text "Gold:" or "Diamond:" staying there, we can find the script that updates it.
        // Usually PlayerEconomyManager handles it. But for now we just change the label if it's static.
        // We can't guarantee it won't be overwritten. We can write another script to override the text formatting.
    }
}
