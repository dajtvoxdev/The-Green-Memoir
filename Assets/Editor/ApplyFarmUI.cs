using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ApplyFarmUI
{
    [MenuItem("Tools/ApplyFarmUI")]
    public static void Apply()
    {
        // Load Assets
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular SDF.asset");
        Texture2D woodTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pixel Art Wooden GUI v1/Wooden Pixel Art GUI 32x32.png");
        
        // Find Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found!");
            return;
        }

        // Apply Font to all TextMeshProUGUI
        TextMeshProUGUI[] allTexts = canvas.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var text in allTexts)
        {
            text.font = font;
            text.color = new Color32(250, 240, 220, 255); // Warm white
            
            // Add subtle outline or shadow if not present
        }

        // Setup Wood Sprite if possible (needs Sprite conversion if it's currently Texture2D without sprite settings)
        string spritePath = AssetDatabase.GetAssetPath(woodTex);
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple; // Assuming the asset pack is a tilesheet
            importer.SaveAndReimport();
        }

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(spritePath) as Sprite[];
        Sprite woodSprite = null;
        if(sprites != null && sprites.Length > 0)
        {
             // Try fetching a generic wooden panel sprite. Using the first one as fallback.
             woodSprite = sprites[1]; 
        }

        // Apply Wood Background to HUDs
        Transform hudTopBar = canvas.transform.Find("HUDTopBarBG");
        if (hudTopBar != null)
        {
             Image bgImage = hudTopBar.GetComponent<Image>();
             if (bgImage != null)
             {
                 if(woodSprite != null) bgImage.sprite = woodSprite;
                 bgImage.color = Color.white; // Reset color to see wood
             }
        }
        
        Debug.Log($"Applied Farm UI: Updated {allTexts.Length} text elements with Cherry Bomb Font.");
    }
}
