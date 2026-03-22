using UnityEngine;
using UnityEditor;

public class CreateSeedItems
{
    [MenuItem("Tools/Moonlit Garden/Create New Seeds")]
    public static void CreateSeeds()
    {
        string folderPath = "Assets/Data/Items";
        
        // Create Carrot Seed
        CreateSeedItem(
            folderPath + "/seed_carrot.asset",
            "seed_carrot",
            "Hạt Cà Rốt",
            "Hạt giống cà rốt tươi ngon, dễ trồng.",
            15,  // buy price
            5    // sell price
        );

        // Create Pumpkin Seed
        CreateSeedItem(
            folderPath + "/seed_pumpkin.asset",
            "seed_pumpkin",
            "Hạt Bí Ngô",
            "Hạt giống bí ngô to tròn, thu hoạch mùa thu.",
            25,
            8
        );

        // Create Cabbage Seed
        CreateSeedItem(
            folderPath + "/seed_cabbage.asset",
            "seed_cabbage",
            "Hạt Bắp Cải",
            "Hạt giống bắp cải xanh mướt.",
            20,
            6
        );

        // Create Cucumber Seed
        CreateSeedItem(
            folderPath + "/seed_cucumber.asset",
            "seed_cucumber",
            "Hạt Dưa Chuột",
            "Hạt giống dưa chuột mát lành.",
            18,
            5
        );

        // Create Onion Seed
        CreateSeedItem(
            folderPath + "/seed_onion.asset",
            "seed_onion",
            "Hạt Hành Tây",
            "Hạt giống hành tây cay thơm.",
            12,
            4
        );

        // Create Potato Seed
        CreateSeedItem(
            folderPath + "/seed_potato.asset",
            "seed_potato",
            "Hạt Khoai Tây",
            "Hạt giống khoai tây bở dẻo.",
            22,
            7
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Created 6 new seed items! Now you need to assign icons for each seed.");
    }

    private static void CreateSeedItem(string path, string itemId, string itemName, string description, int buyPrice, int sellPrice)
    {
        // Check if already exists
        ItemDefinition existing = AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);
        if (existing != null)
        {
            Debug.Log($"Seed {itemName} already exists.");
            return;
        }

        // Create new item definition
        ItemDefinition item = ScriptableObject.CreateInstance<ItemDefinition>();
        item.itemId = itemId;
        item.itemName = itemName;
        item.description = description;
        item.itemType = ItemType.Seed;
        item.stackable = true;
        item.maxStack = 99;
        item.buyPrice = buyPrice;
        
        // Create the asset
        AssetDatabase.CreateAsset(item, path);
        Debug.Log($"Created: {itemName} at {path}");
    }
}
