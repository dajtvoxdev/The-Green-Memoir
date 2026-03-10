using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Automatically localizes all UI Text/TMP_Text elements in loaded scenes.
/// Includes periodic refresh to catch runtime-generated text.
/// </summary>
public class AutoUILocalizer : MonoBehaviour
{
    [Tooltip("How often to re-check runtime text updates.")]
    public float refreshInterval = 0.25f;

    private float nextRefreshTime;
    private LocalizationManager manager;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        if (manager != null)
        {
            manager.OnLanguageChanged -= HandleLanguageChanged;
        }
    }

    private void Start()
    {
        manager = LocalizationManager.Instance;
        if (manager != null)
        {
            manager.OnLanguageChanged += HandleLanguageChanged;
        }

        LocalizeAllUiText();
    }

    private void Update()
    {
        if (Time.unscaledTime < nextRefreshTime)
        {
            return;
        }

        nextRefreshTime = Time.unscaledTime + refreshInterval;
        LocalizeAllUiText();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LocalizeAllUiText();
    }

    private void HandleLanguageChanged(GameLanguage language)
    {
        LocalizeAllUiText();
    }

    private void LocalizeAllUiText()
    {
        if (manager == null)
        {
            manager = LocalizationManager.Instance;
            if (manager == null)
            {
                return;
            }
        }

        Text[] legacyTexts = FindObjectsOfType<Text>(true);
        foreach (Text legacyText in legacyTexts)
        {
            LocalizeLegacyText(legacyText);
        }

        TMP_Text[] tmpTexts = FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text tmpText in tmpTexts)
        {
            LocalizeTmpText(tmpText);
        }
    }

    private void LocalizeLegacyText(Text textComponent)
    {
        if (textComponent == null || string.IsNullOrEmpty(textComponent.text))
        {
            return;
        }

        if (ShouldSkipText(textComponent) || IsRuntimeInputText(textComponent))
        {
            return;
        }

        string localized = manager.Localize(textComponent.text);
        if (!string.Equals(localized, textComponent.text))
        {
            textComponent.text = localized;
        }
    }

    private void LocalizeTmpText(TMP_Text textComponent)
    {
        if (textComponent == null || string.IsNullOrEmpty(textComponent.text))
        {
            return;
        }

        if (ShouldSkipText(textComponent) || IsRuntimeInputText(textComponent))
        {
            return;
        }

        string localized = manager.Localize(textComponent.text);
        if (!string.Equals(localized, textComponent.text))
        {
            textComponent.text = localized;
        }
    }

    private static bool ShouldSkipText(Component textComponent)
    {
        return textComponent.GetComponent<DoNotLocalize>() != null
               || textComponent.GetComponentInParent<DoNotLocalize>() != null;
    }

    private static bool IsRuntimeInputText(Text textComponent)
    {
        InputField inputField = textComponent.GetComponentInParent<InputField>();
        return inputField != null && inputField.textComponent == textComponent;
    }

    private static bool IsRuntimeInputText(TMP_Text textComponent)
    {
        TMP_InputField inputField = textComponent.GetComponentInParent<TMP_InputField>();
        return inputField != null && inputField.textComponent == textComponent;
    }
}
