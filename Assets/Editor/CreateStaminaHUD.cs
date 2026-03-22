using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class CreateStaminaHUD
{
    [MenuItem("Tools/Moonlit Garden/Create Stamina HUD")]
    public static void CreateStaminaHUDMenu()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        // Create StaminaHUD container
        GameObject staminaHUD = new GameObject("StaminaHUD", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        staminaHUD.transform.SetParent(canvas.transform, false);
        
        RectTransform hudRect = staminaHUD.GetComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0, 1);
        hudRect.anchorMax = new Vector2(0, 1);
        hudRect.pivot = new Vector2(0, 1);
        hudRect.anchoredPosition = new Vector2(10, -80);
        hudRect.sizeDelta = new Vector2(200, 40);
        
        Image hudBg = staminaHUD.GetComponent<Image>();
        hudBg.color = new Color(0.235f, 0.157f, 0.118f, 0.784f);
        
        // Create StaminaSlider with proper structure
        GameObject sliderObj = new GameObject("StaminaSlider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(staminaHUD.transform, false);
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = new Vector2(10, 5);
        sliderRect.offsetMax = new Vector2(-10, -5);
        
        Slider slider = sliderObj.GetComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        
        // Create Background
        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        background.transform.SetParent(sliderObj.transform, false);
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        Image bgImg = background.GetComponent<Image>();
        bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        // Create Fill Area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;
        
        // Create Fill
        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        Image fillImg = fill.GetComponent<Image>();
        fillImg.color = new Color(0.392f, 0.784f, 1f, 1f);
        fillImg.raycastTarget = false;
        
        // Setup slider references
        slider.fillRect = fillRect;
        slider.targetGraphic = fillImg;
        
        // Create label text
        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObj.transform.SetParent(staminaHUD.transform, false);
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0, 0.5f);
        labelRect.anchorMax = new Vector2(0, 0.5f);
        labelRect.pivot = new Vector2(0, 0.5f);
        labelRect.anchoredPosition = new Vector2(5, 0);
        labelRect.sizeDelta = new Vector2(60, 20);
        
        Text labelText = labelObj.GetComponent<Text>();
        labelText.text = "Stamina";
        labelText.fontSize = 12;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;
        
        // Add StaminaHUD component
        staminaHUD.AddComponent<StaminaHUD>();
        
        Undo.RegisterCreatedObjectUndo(staminaHUD, "Create StaminaHUD");
        
        Debug.Log("StaminaHUD created successfully!");
        
        Selection.activeGameObject = staminaHUD;
    }
}
