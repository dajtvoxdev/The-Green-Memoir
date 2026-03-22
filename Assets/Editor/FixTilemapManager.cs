using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class FixTilemapManager
{
    [MenuItem("Tools/Moonlit Garden/Fix TilemapManager Reference")]
    public static void FixTmGround()
    {
        // Find TileMapManager
        TileMapManager tmManager = Object.FindFirstObjectByType<TileMapManager>();
        if (tmManager == null)
        {
            Debug.LogError("TileMapManager not found in scene!");
            return;
        }

        // Find Grid
        Grid grid = Object.FindFirstObjectByType<Grid>();
        if (grid == null)
        {
            Debug.LogError("Grid not found in scene!");
            return;
        }

        // Find first Tilemap in Grid
        Tilemap tilemap = grid.GetComponentInChildren<Tilemap>();
        if (tilemap == null)
        {
            Debug.LogError("No Tilemap found in Grid!");
            return;
        }

        // Assign using SerializedObject
        SerializedObject so = new SerializedObject(tmManager);
        SerializedProperty prop = so.FindProperty("tm_Ground");
        if (prop != null)
        {
            prop.objectReferenceValue = tilemap;
            so.ApplyModifiedProperties();
            Debug.Log($"Successfully assigned {tilemap.name} to TileMapManager.tm_Ground!");
        }
        else
        {
            Debug.LogError("Could not find tm_Ground property!");
        }
    }
}
