using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ApplyFarmUI_Phase4
{
    [MenuItem("Tools/ApplyFarmUI_Phase4")]
    public static void Apply()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        
        // 1. Remove LayoutGroups from EconomyHUD and TimeHUD to avoid zero-size bugs
        foreach (var lg in canvas.GetComponentsInChildren<LayoutGroup>(true))
        {
            if (lg.gameObject.name == "EconomyHUD" || lg.gameObject.name == "TimeHUD" || 
                lg.gameObject.name == "Gold" || lg.gameObject.name == "Diamond")
            {
                Object.DestroyImmediate(lg);
            }
        }
        foreach (var csf in canvas.GetComponentsInChildren<ContentSizeFitter>(true))
        {
            if (csf.gameObject.name == "EconomyHUD" || csf.gameObject.name == "TimeHUD" || 
                csf.gameObject.name == "Gold" || csf.gameObject.name == "Diamond")
            {
                Object.DestroyImmediate(csf);
            }
        }

        Transform economyHud = null;
        Transform timeHud = null;

        foreach (var t in canvas.GetComponentsInChildren<Transform>(true))
        {
            if (t.name == "EconomyHUD") economyHud = t;
            if (t.name == "TimeHUD") timeHud = t;
        }

        // 2. Setup Economy manually
        if (economyHud != null)
        {
            RectTransform rootRt = economyHud.GetComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(1,1);
            rootRt.anchorMax = new Vector2(1,1);
            rootRt.pivot = new Vector2(1,1);
            rootRt.anchoredPosition = new Vector2(-40, -40);
            
            Transform goldObj = economyHud.Find("Gold");
            if (goldObj != null) 
            {
                RectTransform rt = goldObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1,0.5f); rt.anchorMax = new Vector2(1,0.5f); rt.pivot = new Vector2(1,0.5f);
                rt.anchoredPosition = new Vector2(-150, 0); // Left of Diamond
                rt.sizeDelta = new Vector2(100, 40);
                
                var t = goldObj.GetComponent<TextMeshProUGUI>();
                if(t) { t.alignment = TextAlignmentOptions.Left; t.enableWordWrapping = false; t.overflowMode = TextOverflowModes.Overflow; }
                
                Transform icon = goldObj.Find("GoldIcon");
                if (icon != null) {
                    RectTransform iRt = icon.GetComponent<RectTransform>();
                    iRt.anchorMin = new Vector2(0,0.5f); iRt.anchorMax = new Vector2(0,0.5f); iRt.pivot = new Vector2(0,0.5f);
                    iRt.anchoredPosition = new Vector2(-40, 0);
                    iRt.sizeDelta = new Vector2(35, 35);
                }
            }
            
            Transform diamondObj = economyHud.Find("Diamond");
            if (diamondObj != null) 
            {
                RectTransform rt = diamondObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(1,0.5f); rt.anchorMax = new Vector2(1,0.5f); rt.pivot = new Vector2(1,0.5f);
                rt.anchoredPosition = new Vector2(0, 0); // Rightmost
                rt.sizeDelta = new Vector2(100, 40);
                
                var t = diamondObj.GetComponent<TextMeshProUGUI>();
                if(t) { t.alignment = TextAlignmentOptions.Left; t.enableWordWrapping = false; t.overflowMode = TextOverflowModes.Overflow; }

                Transform icon = diamondObj.Find("DiamondIcon");
                if (icon != null) {
                    RectTransform iRt = icon.GetComponent<RectTransform>();
                    iRt.anchorMin = new Vector2(0,0.5f); iRt.anchorMax = new Vector2(0,0.5f); iRt.pivot = new Vector2(0,0.5f);
                    iRt.anchoredPosition = new Vector2(-40, 0);
                    iRt.sizeDelta = new Vector2(35, 35);
                }
            }
        }
        
        // 3. Setup Time manually
        if (timeHud != null)
        {
            RectTransform rootRt = timeHud.GetComponent<RectTransform>();
            rootRt.anchorMin = new Vector2(0.5f, 1);
            rootRt.anchorMax = new Vector2(0.5f, 1);
            rootRt.pivot = new Vector2(0.5f,1);
            rootRt.anchoredPosition = new Vector2(0, -40);
            
            Transform icon = timeHud.parent.Find("DayNightIcon");
            if (icon == null) icon = timeHud.Find("DayNightIcon");
            if (icon != null) {
                icon.SetParent(timeHud, false);
                RectTransform iRt = icon.GetComponent<RectTransform>();
                iRt.anchorMin = new Vector2(0.5f,0.5f); iRt.anchorMax = new Vector2(0.5f,0.5f); iRt.pivot = new Vector2(0.5f,0.5f);
                iRt.anchoredPosition = new Vector2(-150, 0);
                iRt.sizeDelta = new Vector2(40, 40);
            }

            Transform timeTxt = timeHud.Find("TimeText");
            if (timeTxt != null) 
            {
                RectTransform rt = timeTxt.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f,0.5f); rt.anchorMax = new Vector2(0.5f,0.5f); rt.pivot = new Vector2(0.5f,0.5f);
                rt.anchoredPosition = new Vector2(-50, 0);
                rt.sizeDelta = new Vector2(150, 40);
                
                var t = timeTxt.GetComponent<TextMeshProUGUI>();
                if(t) { t.alignment = TextAlignmentOptions.Left; t.enableWordWrapping = false; t.overflowMode = TextOverflowModes.Overflow; }
            }
            
            Transform dayTxt = timeHud.Find("DayText");
            if (dayTxt != null) 
            {
                RectTransform rt = dayTxt.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f,0.5f); rt.anchorMax = new Vector2(0.5f,0.5f); rt.pivot = new Vector2(0.5f,0.5f);
                rt.anchoredPosition = new Vector2(100, 0);
                rt.sizeDelta = new Vector2(120, 40);
                
                var t = dayTxt.GetComponent<TextMeshProUGUI>();
                if(t) { t.alignment = TextAlignmentOptions.Left; t.enableWordWrapping = false; t.overflowMode = TextOverflowModes.Overflow; }
            }

            Transform pTxt = timeHud.Find("PeriodText");
            if(pTxt != null) {
                pTxt.gameObject.SetActive(false); // Hide period since we have icon
            }
        }
        
        Debug.Log("ApplyFarmUI_Phase4 completed.");
    }
}
