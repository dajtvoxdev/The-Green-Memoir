using UnityEngine;
using UnityEditor;
using TMPro;

public class CreateFontAsset
{
    [MenuItem("Tools/CreateCherryBombFont")]
    public static void CreateFont()
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular.ttf");
        if (font != null)
        {
            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font);
            AssetDatabase.CreateAsset(fontAsset, "Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular SDF.asset");
            AssetDatabase.SaveAssets();
            Debug.Log("Font Asset Created!");
        }
        else
        {
            Debug.LogError("Font not found!");
        }
    }
}
