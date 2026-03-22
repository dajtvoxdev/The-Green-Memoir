using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

/// <summary>
/// Removes all tiles from Tilemap_Grass that overlap with tiles on Tilemap_Water.
/// This prevents IsWalkable() from incorrectly returning true for water areas
/// (since it checks grass tilemap before water tilemap).
/// </summary>
public class CleanupGrassOverlap
{
    [MenuItem("MoonlitGarden/Fix Water Walkability/Remove Overlapping Grass Tiles")]
    public static void RemoveOverlappingGrassTiles()
    {
        var grassGO = GameObject.Find("Tilemap_Grass");
        var waterGO = GameObject.Find("Tilemap_Water");

        if (grassGO == null) { Debug.LogError("[Cleanup] Tilemap_Grass not found"); return; }
        if (waterGO == null) { Debug.LogError("[Cleanup] Tilemap_Water not found"); return; }

        var grassTilemap = grassGO.GetComponent<Tilemap>();
        var waterTilemap = waterGO.GetComponent<Tilemap>();

        if (grassTilemap == null) { Debug.LogError("[Cleanup] Tilemap_Grass has no Tilemap component"); return; }
        if (waterTilemap == null) { Debug.LogError("[Cleanup] Tilemap_Water has no Tilemap component"); return; }

        // Get bounds of both tilemaps
        grassTilemap.CompressBounds();
        waterTilemap.CompressBounds();

        BoundsInt grassBounds = grassTilemap.cellBounds;
        BoundsInt waterBounds = waterTilemap.cellBounds;

        // Use the union of both bounds to be thorough
        int minX = Mathf.Min(grassBounds.xMin, waterBounds.xMin);
        int maxX = Mathf.Max(grassBounds.xMax, waterBounds.xMax);
        int minY = Mathf.Min(grassBounds.yMin, waterBounds.yMin);
        int maxY = Mathf.Max(grassBounds.yMax, waterBounds.yMax);

        int removedCount = 0;
        int scannedCount = 0;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase grassTile = grassTilemap.GetTile(cell);
                TileBase waterTile = waterTilemap.GetTile(cell);

                if (grassTile != null && waterTile != null)
                {
                    string grassName = grassTile.name;
                    // Remove canal tiles and any water-related tiles from grass
                    if (grassName.Contains("canal") || grassName.Contains("Canal") ||
                        grassName.Contains("water") || grassName.Contains("Water"))
                    {
                        Debug.Log($"[Cleanup] Removing '{grassName}' from Grass at ({x},{y})");
                        grassTilemap.SetTile(cell, null);
                        removedCount++;
                    }
                    scannedCount++;
                }
            }
        }

        // Also check path tilemap for overlaps with water
        var pathGO = GameObject.Find("Tilemap_Path");
        int pathOverlaps = 0;
        if (pathGO != null)
        {
            var pathTilemap = pathGO.GetComponent<Tilemap>();
            if (pathTilemap != null)
            {
                pathTilemap.CompressBounds();
                BoundsInt pathBounds = pathTilemap.cellBounds;
                int pMinX = Mathf.Min(pathBounds.xMin, waterBounds.xMin);
                int pMaxX = Mathf.Max(pathBounds.xMax, waterBounds.xMax);
                int pMinY = Mathf.Min(pathBounds.yMin, waterBounds.yMin);
                int pMaxY = Mathf.Max(pathBounds.yMax, waterBounds.yMax);

                for (int x = pMinX; x <= pMaxX; x++)
                {
                    for (int y = pMinY; y <= pMaxY; y++)
                    {
                        Vector3Int cell = new Vector3Int(x, y, 0);
                        TileBase pathTile = pathTilemap.GetTile(cell);
                        TileBase waterTile = waterTilemap.GetTile(cell);

                        if (pathTile != null && waterTile != null)
                        {
                            pathOverlaps++;
                            Debug.LogWarning($"[Cleanup] Path+Water overlap at ({x},{y}): path='{pathTile.name}', water='{waterTile.name}'");
                        }
                    }
                }
            }
        }

        EditorUtility.SetDirty(grassTilemap);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        string msg = $"Removed {removedCount} canal/water tiles from Tilemap_Grass.\n" +
                     $"Scanned {scannedCount} overlap cells.\n" +
                     $"Path+Water overlaps found: {pathOverlaps} (logged as warnings).";
        Debug.Log("[Cleanup] " + msg);
        EditorUtility.DisplayDialog("Cleanup Complete", msg, "OK");
    }

    [MenuItem("MoonlitGarden/Fix Water Walkability/Scan Overlaps Only (No Changes)")]
    public static void ScanOverlaps()
    {
        var grassGO = GameObject.Find("Tilemap_Grass");
        var waterGO = GameObject.Find("Tilemap_Water");
        var pathGO = GameObject.Find("Tilemap_Path");

        if (grassGO == null || waterGO == null)
        {
            Debug.LogError("[Scan] Missing Tilemap_Grass or Tilemap_Water");
            return;
        }

        var grassTilemap = grassGO.GetComponent<Tilemap>();
        var waterTilemap = waterGO.GetComponent<Tilemap>();

        grassTilemap.CompressBounds();
        waterTilemap.CompressBounds();

        BoundsInt grassBounds = grassTilemap.cellBounds;
        BoundsInt waterBounds = waterTilemap.cellBounds;

        int minX = Mathf.Min(grassBounds.xMin, waterBounds.xMin);
        int maxX = Mathf.Max(grassBounds.xMax, waterBounds.xMax);
        int minY = Mathf.Min(grassBounds.yMin, waterBounds.yMin);
        int maxY = Mathf.Max(grassBounds.yMax, waterBounds.yMax);

        int grassWaterOverlaps = 0;
        int pathWaterOverlaps = 0;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                TileBase grassTile = grassTilemap.GetTile(cell);
                TileBase waterTile = waterTilemap.GetTile(cell);

                if (grassTile != null && waterTile != null)
                {
                    Debug.Log($"[Scan] Grass+Water at ({x},{y}): grass='{grassTile.name}', water='{waterTile.name}'");
                    grassWaterOverlaps++;
                }
            }
        }

        if (pathGO != null)
        {
            var pathTilemap = pathGO.GetComponent<Tilemap>();
            if (pathTilemap != null)
            {
                pathTilemap.CompressBounds();
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        Vector3Int cell = new Vector3Int(x, y, 0);
                        TileBase pathTile = pathTilemap.GetTile(cell);
                        TileBase waterTile = waterTilemap.GetTile(cell);

                        if (pathTile != null && waterTile != null)
                        {
                            Debug.Log($"[Scan] Path+Water at ({x},{y}): path='{pathTile.name}', water='{waterTile.name}'");
                            pathWaterOverlaps++;
                        }
                    }
                }
            }
        }

        string msg = $"Grass+Water overlaps: {grassWaterOverlaps}\nPath+Water overlaps: {pathWaterOverlaps}";
        Debug.Log("[Scan] " + msg);
        EditorUtility.DisplayDialog("Overlap Scan", msg, "OK");
    }
}
