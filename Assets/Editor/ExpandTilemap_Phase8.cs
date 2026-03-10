using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ExpandTilemap_Phase8
{
    [MenuItem("Tools/ExpandTilemap_Phase8")]
    public static void Apply()
    {
        Grid grid = Object.FindFirstObjectByType<Grid>();
        if (grid == null) {
            Debug.LogError("No Grid found!"); return;
        }

        Tilemap baseMap = null;
        foreach (var tm in grid.GetComponentsInChildren<Tilemap>())
        {
            if (tm.name.ToLower().Contains("ground") || tm.name.ToLower().Contains("base") || tm.name.ToLower().Contains("dirt"))
            {
                baseMap = tm;
                break;
            }
        }

        if (baseMap == null)
        {
            var tms = grid.GetComponentsInChildren<Tilemap>();
            if (tms.Length > 0) baseMap = tms[0];
        }

        if (baseMap == null)
        {
            Debug.LogError("No Tilemaps found under Grid.");
            return;
        }

        System.Collections.Generic.Dictionary<TileBase, int> countMap = new System.Collections.Generic.Dictionary<TileBase, int>();
        BoundsInt bounds = baseMap.cellBounds;
        TileBase mostUsedTile = null;
        int maxCount = 0;

        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase t = baseMap.GetTile(pos);
            if (t != null)
            {
                if (!countMap.ContainsKey(t)) countMap[t] = 0;
                countMap[t]++;
                if (countMap[t] > maxCount)
                {
                    maxCount = countMap[t];
                    mostUsedTile = t;
                }
            }
        }

        if (mostUsedTile == null)
        {
            Debug.LogError("The Tilemap is completely empty!");
            return;
        }

        int filledCount = 0;
        int areaSize = 100; // 200x200 grid
        for (int x = -areaSize; x <= areaSize; x++)
        {
            for (int y = -areaSize; y <= areaSize; y++)
            {
                Vector3Int p = new Vector3Int(x, y, 0);
                if (!baseMap.HasTile(p))
                {
                    baseMap.SetTile(p, mostUsedTile);
                    filledCount++;
                }
            }
        }

        Debug.Log($"Filled {filledCount} cells on {baseMap.name} using {mostUsedTile.name}");
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Revert background to see tiles
            mainCam.backgroundColor = new Color32(49, 77, 121, 255); 
        }
    }
}
