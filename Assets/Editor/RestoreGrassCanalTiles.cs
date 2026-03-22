using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

/// <summary>
/// Restores canal tiles on Tilemap_Grass that were accidentally removed.
/// Paints RuleTile_Canal on Tilemap_Grass wherever Tilemap_Water has a tile
/// but Tilemap_Grass does not.
/// </summary>
public class RestoreGrassCanalTiles
{
    [MenuItem("MoonlitGarden/Fix Water Walkability/Restore Grass Canal Tiles")]
    public static void Restore()
    {
        var grassGO = GameObject.Find("Tilemap_Grass");
        var waterGO = GameObject.Find("Tilemap_Water");

        if (grassGO == null) { Debug.LogError("[Restore] Tilemap_Grass not found"); return; }
        if (waterGO == null) { Debug.LogError("[Restore] Tilemap_Water not found"); return; }

        var grassTilemap = grassGO.GetComponent<Tilemap>();
        var waterTilemap = waterGO.GetComponent<Tilemap>();

        // Load the RuleTile_Canal asset
        var canalTile = AssetDatabase.LoadAssetAtPath<TileBase>("Assets/Sprites/Tilesets/RuleTile_Canal.asset");
        if (canalTile == null)
        {
            Debug.LogError("[Restore] RuleTile_Canal.asset not found at Assets/Sprites/Tilesets/");
            return;
        }

        waterTilemap.CompressBounds();
        BoundsInt waterBounds = waterTilemap.cellBounds;

        int restored = 0;
        for (int x = waterBounds.xMin; x <= waterBounds.xMax; x++)
        {
            for (int y = waterBounds.yMin; y <= waterBounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase waterTile = waterTilemap.GetTile(cell);
                TileBase grassTile = grassTilemap.GetTile(cell);

                // If water has a tile but grass doesn't, restore the canal tile on grass
                if (waterTile != null && grassTile == null)
                {
                    grassTilemap.SetTile(cell, canalTile);
                    restored++;
                }
            }
        }

        EditorUtility.SetDirty(grassTilemap);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        string msg = "Restored " + restored + " canal tiles on Tilemap_Grass.";
        Debug.Log("[Restore] " + msg);
        EditorUtility.DisplayDialog("Restore Complete", msg, "OK");
    }
}
