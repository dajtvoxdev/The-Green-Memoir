using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// One-shot editor script to wire tb_TilledSoil and tm_Forest on TileMapManager
/// by copying references from PlayerFarmController (which already has them assigned).
/// </summary>
public class WireTileMapManagerRefs
{
    [MenuItem("Tools/Wire TileMapManager References")]
    public static void Wire()
    {
        // Find PlayerFarmController (source of truth for references)
        var player = GameObject.FindObjectOfType<PlayerFarmController>();
        if (player == null)
        {
            Debug.LogError("WireTileMapManagerRefs: PlayerFarmController not found!");
            return;
        }

        // Find TileMapManager
        var tmManager = GameObject.FindObjectOfType<TileMapManager>();
        if (tmManager == null)
        {
            Debug.LogError("WireTileMapManagerRefs: TileMapManager not found!");
            return;
        }

        // Use SerializedObject to set references (works with scene serialization)
        var so = new SerializedObject(tmManager);

        // Wire tb_TilledSoil
        var tilledProp = so.FindProperty("tb_TilledSoil");
        if (tilledProp != null && player.tb_TilledSoil != null)
        {
            tilledProp.objectReferenceValue = player.tb_TilledSoil;
            Debug.Log($"Wired tb_TilledSoil = {player.tb_TilledSoil.name}");
        }
        else
        {
            Debug.LogWarning("tb_TilledSoil property or source reference not found");
        }

        // Wire tm_Forest from PlayerFarmController
        var forestTmProp = so.FindProperty("tm_Forest");
        if (forestTmProp != null && player.tm_Forest != null)
        {
            forestTmProp.objectReferenceValue = player.tm_Forest;
            Debug.Log($"Wired tm_Forest = {player.tm_Forest.name}");
        }
        else
        {
            Debug.LogWarning("tm_Forest property or source reference not found");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(tmManager);

        Debug.Log("WireTileMapManagerRefs: Done! Save the scene to persist.");
    }
}
