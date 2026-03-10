using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Creates a small overlay button to toggle between EN and VI at runtime.
/// Also supports keyboard toggle with F10.
/// </summary>
public class LanguageToggleUI : MonoBehaviour
{
    public KeyCode toggleKey = KeyCode.F10;
    public string visibleSceneName = "LoginScene";

    private Button toggleButton;
    private Text toggleLabel;
    private LocalizationManager manager;
    private GameObject toggleCanvasRoot;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        manager = LocalizationManager.Instance;
        if (manager == null)
        {
            return;
        }

        CreateToggleUi();
        SceneManager.sceneLoaded += HandleSceneLoaded;
        manager.OnLanguageChanged += HandleLanguageChanged;
        HandleLanguageChanged(manager.CurrentLanguage);
        RefreshVisibility();
    }

    private void Update()
    {
        if (manager != null && IsVisibleInCurrentScene() && Input.GetKeyDown(toggleKey))
        {
            manager.ToggleLanguage();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (manager != null)
        {
            manager.OnLanguageChanged -= HandleLanguageChanged;
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshVisibility();
    }

    private void HandleLanguageChanged(GameLanguage language)
    {
        if (toggleLabel == null)
        {
            return;
        }

        toggleLabel.text = language == GameLanguage.Vietnamese
            ? "VI"
            : "EN";
    }

    private void CreateToggleUi()
    {
        GameObject canvasGo = new GameObject("LanguageToggleCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(DoNotLocalize));
        canvasGo.transform.SetParent(transform, false);
        toggleCanvasRoot = canvasGo;

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject buttonGo = new GameObject("LanguageToggleButton",
            typeof(RectTransform), typeof(Image), typeof(Button), typeof(DoNotLocalize));
        buttonGo.transform.SetParent(canvasGo.transform, false);

        RectTransform buttonRt = buttonGo.GetComponent<RectTransform>();
        buttonRt.anchorMin = new Vector2(1f, 1f);
        buttonRt.anchorMax = new Vector2(1f, 1f);
        buttonRt.pivot = new Vector2(1f, 1f);
        buttonRt.sizeDelta = new Vector2(72f, 40f);
        buttonRt.anchoredPosition = new Vector2(-24f, -24f);

        Image buttonImage = buttonGo.GetComponent<Image>();
        buttonImage.color = new Color(0.14f, 0.18f, 0.2f, 0.86f);

        toggleButton = buttonGo.GetComponent<Button>();
        ColorBlock colors = toggleButton.colors;
        colors.normalColor = new Color(0.14f, 0.18f, 0.2f, 0.86f);
        colors.highlightedColor = new Color(0.2f, 0.26f, 0.3f, 0.9f);
        colors.pressedColor = new Color(0.1f, 0.14f, 0.18f, 0.92f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
        toggleButton.colors = colors;
        toggleButton.onClick.AddListener(OnToggleClicked);

        GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text), typeof(DoNotLocalize));
        labelGo.transform.SetParent(buttonGo.transform, false);

        RectTransform labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        toggleLabel = labelGo.GetComponent<Text>();
        Font runtimeFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (runtimeFont == null)
        {
            runtimeFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        if (runtimeFont == null)
        {
            runtimeFont = Font.CreateDynamicFontFromOSFont("Arial", 20);
        }
        toggleLabel.font = runtimeFont;
        toggleLabel.fontSize = 20;
        toggleLabel.alignment = TextAnchor.MiddleCenter;
        toggleLabel.color = new Color(0.95f, 0.98f, 1f, 1f);
        toggleLabel.resizeTextForBestFit = true;
        toggleLabel.resizeTextMinSize = 12;
        toggleLabel.resizeTextMaxSize = 24;
        toggleLabel.text = "VI";
    }

    private void OnToggleClicked()
    {
        if (IsVisibleInCurrentScene())
        {
            manager?.ToggleLanguage();
        }
    }

    private void RefreshVisibility()
    {
        if (toggleCanvasRoot == null)
        {
            return;
        }

        toggleCanvasRoot.SetActive(IsVisibleInCurrentScene());
    }

    private bool IsVisibleInCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        return activeScene.IsValid() && activeScene.name == visibleSceneName;
    }
}
