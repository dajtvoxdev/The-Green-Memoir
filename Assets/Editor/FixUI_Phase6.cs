using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FixUI_Phase6
{
    [MenuItem("Tools/FixUI_Phase6")]
    public static void Apply()
    {
        // 1. Fix Camera Background (Hide default unity blue)
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.clearFlags = CameraClearFlags.SolidColor;
            mainCam.backgroundColor = new Color32(94, 62, 43, 255); // Farm dirt brown fallback
        }

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // 2. Hide unwanted black backgrounds causing dark layout issues
        string[] uiTargets = {"EconomyHUD", "TimeHUD", "Gold", "Diamond", "TimeText", "DayText", "HUDTopBarBG", "UI Name Wizard", "UI Information In Game"};
        foreach (var img in canvas.GetComponentsInChildren<Image>(true))
        {
            foreach (var tStr in uiTargets)
            {
                if (img.gameObject.name == tStr)
                {
                    img.enabled = false;
                }
            }
        }

        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular SDF.asset");

        // 3. Fix Texts Invisible
        foreach (var t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (t.gameObject.name == "Gold" || t.gameObject.name == "Diamond" || 
                t.gameObject.name == "TimeText" || t.gameObject.name == "DayText")
            {
                t.font = fontAsset;
                t.enableWordWrapping = false;
                t.overflowMode = TextOverflowModes.Overflow;
                t.fontSize = 32;
                t.color = Color.white;
                t.fontStyle = FontStyles.Bold;
                
                var rt = t.rectTransform;
                Vector3 lp = rt.localPosition;
                lp.z = 0; // Fix Z depth so text doesn't fall behind UI or canvas
                rt.localPosition = lp;
                rt.localScale = Vector3.one;

                // Ensure it has width and height
                rt.sizeDelta = new Vector2(250, 100);
            }
        }
        
        Debug.Log("FixUI_Phase6 UI Rescue completed.");
    }
}
