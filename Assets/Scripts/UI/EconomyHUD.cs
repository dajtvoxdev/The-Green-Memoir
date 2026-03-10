using UnityEngine;
using TMPro;

/// <summary>
/// HUD element displaying Gold and Diamond counts.
/// Subscribes to PlayerEconomyManager events for live updates.
///
/// Phase 2 Feature (#9): Visual economy display.
/// </summary>
public class EconomyHUD : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text showing Gold amount.")]
    public TMP_Text goldText;

    [Tooltip("Text showing Diamond amount.")]
    public TMP_Text diamondText;

    void Start()
    {
        RefreshDisplay();

        if (PlayerEconomyManager.Instance != null)
        {
            PlayerEconomyManager.Instance.OnGoldChanged += OnGoldChanged;
            PlayerEconomyManager.Instance.OnDiamondChanged += OnDiamondChanged;
        }
    }

    private void OnGoldChanged(int newGold)
    {
        if (goldText != null)
        {
            goldText.text = newGold.ToString();
        }
    }

    private void OnDiamondChanged(int newDiamond)
    {
        if (diamondText != null)
        {
            diamondText.text = newDiamond.ToString();
        }
    }

    /// <summary>
    /// Refreshes the display from current data.
    /// </summary>
    public void RefreshDisplay()
    {
        if (LoadDataManager.userInGame != null)
        {
            OnGoldChanged(LoadDataManager.userInGame.Gold);
            OnDiamondChanged(LoadDataManager.userInGame.Diamond);
        }
    }

    void OnDestroy()
    {
        if (PlayerEconomyManager.Instance != null)
        {
            PlayerEconomyManager.Instance.OnGoldChanged -= OnGoldChanged;
            PlayerEconomyManager.Instance.OnDiamondChanged -= OnDiamondChanged;
        }
    }
}
