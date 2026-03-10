using UnityEditor;
using UnityEngine;

/// <summary>
/// Debug script: In ra tọa độ sprite trên spritesheet cho từng tile index.
/// Giúp xác định đúng tile nào ứng với phần nào trên spritesheet.
/// Chạy 1 lần rồi xóa.
/// </summary>
public class DebugSpriteMapping
{
    [MenuItem("Tools/DebugSpriteMapping")]
    public static void Run()
    {
        // Load spritesheet và lấy tất cả sub-sprites
        string path = "Assets/ElvGames/Farm Game Assets/Tilesets/Grasslands/Sprites/FG_Grasslands_Spring.png";
        Object[] allSprites = AssetDatabase.LoadAllAssetsAtPath(path);
        
        Debug.Log($"Tổng assets: {allSprites.Length}");
        
        // Chỉ in một số index quan trọng để xác định pattern
        int[] checkIndices = { 0, 1, 5, 10, 20, 25, 26, 30, 32, 36, 37, 40, 45, 46, 50, 52, 53, 55, 
                               64, 65, 69, 70, 72, 73, 74, 75, 80, 85, 90, 92, 93, 95,
                               100, 115, 116, 128, 130, 140, 150, 160, 169, 170, 180, 183, 184, 187, 188,
                               200, 250, 300, 350, 400, 435, 436, 438, 455, 456, 458, 475, 476, 478,
                               496, 497, 500, 550, 600, 640, 643, 660, 680, 700, 703, 750, 800 };
        
        foreach (int idx in checkIndices)
        {
            string tileName = $"FG_Grasslands_Spring_{idx}";
            
            // Tìm sprite tương ứng
            foreach (Object obj in allSprites)
            {
                if (obj is Sprite s && s.name == tileName)
                {
                    Rect r = s.rect;
                    Debug.Log($"[{idx}] {tileName}: x={r.x} y={r.y} w={r.width} h={r.height} | row={Mathf.FloorToInt((512 - r.y - r.height) / 16)} col={Mathf.FloorToInt(r.x / 16)}");
                    break;
                }
            }
        }
    }
}
