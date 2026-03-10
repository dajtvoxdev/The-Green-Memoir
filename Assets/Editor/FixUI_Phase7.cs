using UnityEditor;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FixUI_Phase7
{
    [MenuItem("Tools/FixUI_Phase7")]
    public static void Apply()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        // Load Font and ensure it exists
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular SDF.asset");
        if (fontAsset == null) 
        {
            Debug.LogError("Error: Không tìm thấy CherryBombOne-Regular SDF.asset. Đảm bảo bạn đã dọn dẹp hoặc khởi tạo TMP_FontAsset.");
            return;
        }

        foreach (var t in canvas.GetComponentsInChildren<TextMeshProUGUI>(true))
        {
            if (t.gameObject.name == "Gold" || t.gameObject.name == "Diamond" || 
                t.gameObject.name == "TimeText" || t.gameObject.name == "DayText" ||
                t.gameObject.name.Contains("Text"))
            {
                t.font = fontAsset;
                
                // CRITICAL FIX: Script gán font không hiển thị nếu không cập nhật SharedMaterial cho Mesh của TMP
                t.fontSharedMaterial = fontAsset.material; 
                
                // Extra failsafes
                t.color = Color.white;
                t.fontSize = 32;
                t.enabled = false; t.enabled = true; // Hard refresh
                t.SetAllDirty(); // Force Unity to re-render the mesh
            }
        }
        
        Debug.Log("FixUI_Phase7: TextMeshPro material synchronization completed.");
    }
}
