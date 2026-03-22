using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Wires EconomyHUD component references: goldText and diamondText.
/// </summary>
public class WireEconomyHUD
{
    [MenuItem("MoonlitGarden/Wire EconomyHUD References")]
    public static void Wire()
    {
        // Find GoldHUD which has the EconomyHUD component
        var goldHUD = GameObject.Find("GoldHUD");
        if (goldHUD == null)
        {
            Debug.LogError("[WireEconomyHUD] GoldHUD not found in scene");
            return;
        }

        var economyHUD = goldHUD.GetComponent<EconomyHUD>();
        if (economyHUD == null)
        {
            Debug.LogError("[WireEconomyHUD] EconomyHUD component not found on GoldHUD");
            return;
        }

        // Wire goldText → GoldHUD/GoldText
        var goldTextGO = goldHUD.transform.Find("GoldText");
        if (goldTextGO != null)
        {
            economyHUD.goldText = goldTextGO.GetComponent<TMP_Text>();
            Debug.Log("[WireEconomyHUD] goldText wired to " + goldTextGO.name);
        }
        else
        {
            Debug.LogWarning("[WireEconomyHUD] GoldText child not found under GoldHUD");
        }

        // Wire diamondText → DiamondHUD/DiamondText
        var diamondHUD = GameObject.Find("DiamondHUD");
        if (diamondHUD != null)
        {
            var diamondTextGO = diamondHUD.transform.Find("DiamondText");
            if (diamondTextGO != null)
            {
                economyHUD.diamondText = diamondTextGO.GetComponent<TMP_Text>();
                Debug.Log("[WireEconomyHUD] diamondText wired to " + diamondTextGO.name);
            }
            else
            {
                Debug.LogWarning("[WireEconomyHUD] DiamondText child not found under DiamondHUD");
            }
        }
        else
        {
            Debug.LogWarning("[WireEconomyHUD] DiamondHUD not found in scene");
        }

        EditorUtility.SetDirty(economyHUD);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("[WireEconomyHUD] Done! Gold=" + (economyHUD.goldText != null) +
                  " Diamond=" + (economyHUD.diamondText != null));
    }
}
