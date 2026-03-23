using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Editor utility that creates harvest ItemDefinition assets for all CropDefinitions
/// and wires them into the GeneralStore ShopCatalog.
///
/// Run: Tools > Moonlit Garden > Create Harvest Items & Wire Shop
///
/// What it does:
/// 1. Scans all CropDefinition assets in Assets/Data/Crops/
/// 2. For each crop, creates a matching ItemDefinition asset (e.g., crop_tomato.asset)
///    in Assets/Data/Items/ with itemType = Crop, sellPrice from CropDefinition
/// 3. Updates the GeneralStore ShopCatalog to include harvest items (sell-only)
/// 4. Ensures all 18 seed ItemDefinitions are also in the shop
/// </summary>
public class CreateHarvestItems : EditorWindow
{
    private const string ITEMS_PATH = "Assets/Data/Items";
    private const string CROPS_PATH = "Assets/Data/Crops";
    private const string SHOP_PATH = "Assets/Data/Shop/GeneralStore.asset";

    [MenuItem("Tools/Moonlit Garden/Create Harvest Items & Wire Shop")]
    public static void Execute()
    {
        Debug.Log("=== CreateHarvestItems: Starting ===");

        // Ensure output directory exists
        if (!AssetDatabase.IsValidFolder(ITEMS_PATH))
        {
            Debug.LogError($"CreateHarvestItems: Items folder not found at {ITEMS_PATH}");
            return;
        }

        // Step 1: Load all CropDefinitions
        string[] cropGuids = AssetDatabase.FindAssets("t:CropDefinition", new[] { CROPS_PATH });
        if (cropGuids.Length == 0)
        {
            Debug.LogError("CreateHarvestItems: No CropDefinition assets found!");
            return;
        }

        Debug.Log($"CreateHarvestItems: Found {cropGuids.Length} CropDefinition assets");

        // Step 2: Create harvest ItemDefinitions
        var harvestItems = new List<ItemDefinition>();
        int created = 0;
        int skipped = 0;

        foreach (string guid in cropGuids)
        {
            string cropAssetPath = AssetDatabase.GUIDToAssetPath(guid);
            CropDefinition cropDef = AssetDatabase.LoadAssetAtPath<CropDefinition>(cropAssetPath);
            if (cropDef == null) continue;

            string harvestItemId = cropDef.harvestItemId;
            if (string.IsNullOrEmpty(harvestItemId))
            {
                Debug.LogWarning($"CreateHarvestItems: CropDefinition '{cropDef.cropId}' has no harvestItemId, skipping.");
                continue;
            }

            string itemAssetPath = $"{ITEMS_PATH}/{harvestItemId}.asset";

            // Check if already exists
            ItemDefinition existingItem = AssetDatabase.LoadAssetAtPath<ItemDefinition>(itemAssetPath);
            if (existingItem != null)
            {
                // Update existing item with latest crop data
                Undo.RecordObject(existingItem, "Update harvest ItemDefinition");
                existingItem.itemId = harvestItemId;
                existingItem.itemName = cropDef.cropName;
                existingItem.description = $"{cropDef.cropName} thu hoạch tươi ngon.";
                existingItem.itemType = ItemType.Crop;
                existingItem.stackable = true;
                existingItem.maxStack = 99;
                existingItem.buyPrice = 0; // Cannot buy harvested crops
                existingItem.sellPrice = cropDef.sellPrice;
                existingItem.cropId = cropDef.cropId;
                existingItem.yieldMin = cropDef.yieldMin;
                existingItem.yieldMax = cropDef.yieldMax;

                // Try to use the harvestable stage sprite as icon
                Sprite harvestSprite = cropDef.GetStageSprite(GrowthStage.Harvestable);
                if (harvestSprite != null)
                {
                    existingItem.icon = harvestSprite;
                }

                EditorUtility.SetDirty(existingItem);
                harvestItems.Add(existingItem);
                skipped++;
                Debug.Log($"CreateHarvestItems: Updated existing '{harvestItemId}'");
                continue;
            }

            // Create new ItemDefinition
            ItemDefinition newItem = ScriptableObject.CreateInstance<ItemDefinition>();
            newItem.itemId = harvestItemId;
            newItem.itemName = cropDef.cropName;
            newItem.description = $"{cropDef.cropName} thu hoạch tươi ngon.";
            newItem.itemType = ItemType.Crop;
            newItem.stackable = true;
            newItem.maxStack = 99;
            newItem.buyPrice = 0;
            newItem.sellPrice = cropDef.sellPrice;
            newItem.cropId = cropDef.cropId;
            newItem.yieldMin = cropDef.yieldMin;
            newItem.yieldMax = cropDef.yieldMax;

            // Use harvestable stage sprite as icon
            Sprite harvestSprite2 = cropDef.GetStageSprite(GrowthStage.Harvestable);
            if (harvestSprite2 != null)
            {
                newItem.icon = harvestSprite2;
            }

            AssetDatabase.CreateAsset(newItem, itemAssetPath);
            harvestItems.Add(newItem);
            created++;
            Debug.Log($"CreateHarvestItems: Created '{harvestItemId}' (sellPrice={cropDef.sellPrice})");
        }

        Debug.Log($"CreateHarvestItems: Created {created}, updated {skipped} harvest ItemDefinitions");

        // Step 3: Load all seed ItemDefinitions
        string[] seedGuids = AssetDatabase.FindAssets("t:ItemDefinition", new[] { ITEMS_PATH });
        var seedItems = new List<ItemDefinition>();
        foreach (string guid in seedGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
            if (item != null && item.itemType == ItemType.Seed)
            {
                seedItems.Add(item);
            }
        }
        Debug.Log($"CreateHarvestItems: Found {seedItems.Count} seed ItemDefinitions");

        // Step 4: Wire into GeneralStore ShopCatalog
        ShopCatalog shop = AssetDatabase.LoadAssetAtPath<ShopCatalog>(SHOP_PATH);
        if (shop == null)
        {
            Debug.LogError($"CreateHarvestItems: GeneralStore not found at {SHOP_PATH}");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return;
        }

        Undo.RecordObject(shop, "Wire harvest items into GeneralStore");

        // Build a set of existing item IDs in the shop
        var existingIds = new HashSet<string>();
        var currentEntries = new List<ShopEntry>();
        if (shop.entries != null)
        {
            foreach (var entry in shop.entries)
            {
                if (entry.item != null)
                {
                    existingIds.Add(entry.item.itemId);
                    currentEntries.Add(entry);
                }
            }
        }

        // Add missing seeds to shop
        int seedsAdded = 0;
        foreach (var seed in seedItems)
        {
            if (existingIds.Contains(seed.itemId)) continue;

            currentEntries.Add(new ShopEntry
            {
                item = seed,
                buyPrice = seed.buyPrice,
                sellPrice = seed.sellPrice,
                available = true,
                maxStock = -1
            });
            existingIds.Add(seed.itemId);
            seedsAdded++;
            Debug.Log($"CreateHarvestItems: Added seed '{seed.itemId}' to shop (buy={seed.buyPrice})");
        }

        // Add harvest items to shop (sell-only: buyPrice = 0)
        int harvestAdded = 0;
        foreach (var harvest in harvestItems)
        {
            if (existingIds.Contains(harvest.itemId)) continue;

            currentEntries.Add(new ShopEntry
            {
                item = harvest,
                buyPrice = 0, // Cannot buy harvested crops from shop
                sellPrice = harvest.sellPrice,
                available = true,
                maxStock = -1
            });
            existingIds.Add(harvest.itemId);
            harvestAdded++;
            Debug.Log($"CreateHarvestItems: Added harvest '{harvest.itemId}' to shop (sell={harvest.sellPrice})");
        }

        shop.entries = currentEntries.ToArray();
        EditorUtility.SetDirty(shop);

        // Save everything
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"=== CreateHarvestItems: Complete! ===");
        Debug.Log($"  Harvest items: {created} created, {skipped} updated");
        Debug.Log($"  Seeds added to shop: {seedsAdded}");
        Debug.Log($"  Harvest items added to shop: {harvestAdded}");
        Debug.Log($"  Total shop entries: {shop.entries.Length}");

        EditorUtility.DisplayDialog(
            "Create Harvest Items",
            $"Done!\n\n" +
            $"Harvest ItemDefinitions: {created} created, {skipped} updated\n" +
            $"Seeds added to shop: {seedsAdded}\n" +
            $"Harvest items added to shop: {harvestAdded}\n" +
            $"Total shop entries: {shop.entries.Length}",
            "OK"
        );
    }
}
