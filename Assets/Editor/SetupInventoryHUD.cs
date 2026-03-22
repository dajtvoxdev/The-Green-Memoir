using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SetupInventoryHUD
{
    [MenuItem("Tools/Moonlit Garden/Setup Inventory HUD")]
    static void SetupInventory()
    {
        // Configure sprites
        ConfigureSprite("Assets/UI/InventoryPanel_BG.png");
        ConfigureSprite("Assets/UI/InventoryItemSlot.png");

        // Find InventoryHUD
        GameObject inventoryHUD = GameObject.Find("InventoryHUD");
        if (inventoryHUD == null)
        {
            Debug.LogError("InventoryHUD not found in scene!");
            return;
        }

        // Add or get Image component for background
        Image bgImage = inventoryHUD.GetComponent<Image>();
        if (bgImage == null)
        {
            bgImage = inventoryHUD.AddComponent<Image>();
        }

        // Load and assign background sprite
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/InventoryPanel_BG.png");
        if (bgSprite != null)
        {
            bgImage.sprite = bgSprite;
            bgImage.type = Image.Type.Sliced;
            bgImage.color = Color.white;
        }

        // Find or create item slots container
        Transform slotsContainer = inventoryHUD.transform.Find("SlotsContainer");
        if (slotsContainer == null)
        {
            GameObject container = new GameObject("SlotsContainer");
            container.transform.SetParent(inventoryHUD.transform);
            slotsContainer = container.transform;
        }

        // Setup slot images
        Sprite slotSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/InventoryItemSlot.png");
        if (slotSprite != null)
        {
            // Find all CellItemData children and setup their images
            foreach (Transform child in slotsContainer)
            {
                Image slotImage = child.GetComponent<Image>();
                if (slotImage == null)
                {
                    slotImage = child.gameObject.AddComponent<Image>();
                }
                slotImage.sprite = slotSprite;
                slotImage.type = Image.Type.Sliced;
            }
        }

        // Configure RectTransform
        RectTransform rt = inventoryHUD.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 100);
            rt.sizeDelta = new Vector2(500, 120);
        }

        Debug.Log("✅ Inventory HUD setup complete!");
        Selection.activeGameObject = inventoryHUD;
    }

    static void ConfigureSprite(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 100;
            
            // Enable read/write for UI slicing
            importer.isReadable = true;
            
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }
}
