using UnityEngine;

/// <summary>
/// Creates the localization runtime system automatically before first scene loads.
/// </summary>
public static class LocalizationBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (Object.FindObjectOfType<LocalizationManager>() != null)
        {
            return;
        }

        GameObject localizationRoot = new GameObject("LocalizationSystem");
        Object.DontDestroyOnLoad(localizationRoot);
        localizationRoot.AddComponent<LocalizationManager>();
        localizationRoot.AddComponent<AutoUILocalizer>();
        localizationRoot.AddComponent<LanguageToggleUI>();
    }
}
