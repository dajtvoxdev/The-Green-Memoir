using UnityEngine;
using UnityEditor;

/// <summary>
/// Creates/updates all 18 CropDefinition + ItemDefinition assets with tier-based economy.
/// Menu: Tools > Setup All Crops (Tier System)
/// </summary>
public class SetupAllCrops
{
    [MenuItem("Tools/Setup All Crops (Tier System)")]
    public static void Setup()
    {
        // ==================== TIER 1: COMMON (fast growth, cheap) ====================
        UpsertCrop("bean", "Đậu", "Đậu xanh dễ trồng. Cây khởi đầu hoàn hảo.",
            new float[] { 15, 20, 20, 15 }, 3, 5, 10, false, "Bean");
        UpsertCrop("corn", "Ngô", "Ngô ngọt dễ trồng. Thu hoạch nhiều bắp.",
            new float[] { 18, 22, 22, 18 }, 4, 7, 12, false, "Corn");
        UpsertCrop("wheat", "Lúa Mì", "Lúa mì vàng óng. Nền tảng của nông trại.",
            new float[] { 20, 25, 25, 20 }, 3, 5, 15, false, "Wheat");
        UpsertCrop("carrot", "Cà Rốt", "Cà rốt tươi giòn ngọt.",
            new float[] { 22, 28, 28, 22 }, 2, 4, 18, false, "Carrot");
        UpsertCrop("potato", "Khoai Tây", "Khoai tây bùi ngon. Thu hoạch nhiều củ.",
            new float[] { 24, 30, 30, 26 }, 3, 6, 16, false, "Potato");
        UpsertCrop("onion", "Hành Tây", "Hành tây thơm nồng.",
            new float[] { 26, 32, 32, 30 }, 2, 4, 20, false, "Onion");

        // ==================== TIER 2: UNCOMMON (medium growth, medium price) ====================
        UpsertCrop("cucumber", "Dưa Chuột", "Dưa chuột mát lành. Cho thu hoạch nhiều lần.",
            new float[] { 40, 50, 50, 40 }, 1, 3, 22, true, "Cucumber");
        UpsertCrop("tomato", "Cà Chua", "Cà chua chín mọng. Giá trị cao.",
            new float[] { 45, 55, 55, 45 }, 2, 4, 30, false, "Tomato");
        UpsertCrop("cabbage", "Bắp Cải", "Bắp cải xanh tươi giòn.",
            new float[] { 50, 60, 60, 50 }, 1, 3, 35, false, "Cabbage");
        UpsertCrop("chili", "Ớt", "Ớt cay nồng. Cho thu hoạch nhiều lần.",
            new float[] { 42, 52, 52, 44 }, 2, 5, 28, true, "Chili");
        UpsertCrop("eggplant", "Cà Tím", "Cà tím tươi ngon bổ dưỡng.",
            new float[] { 48, 58, 56, 48 }, 2, 3, 32, false, "Eggplant");

        // ==================== TIER 3: RARE (slow growth, expensive) ====================
        UpsertCrop("garlic", "Tỏi", "Tỏi thơm cay. Gia vị quý.",
            new float[] { 75, 90, 90, 85 }, 2, 4, 55, false, "Garlic");
        UpsertCrop("pumpkin", "Bí Đỏ", "Bí đỏ to tròn. Giá bán cao.",
            new float[] { 80, 100, 100, 80 }, 1, 2, 65, false, "Pumpkin");
        UpsertCrop("strawberry", "Dâu Tây", "Dâu tây ngọt ngào. Thu hoạch nhiều lần.",
            new float[] { 85, 105, 100, 90 }, 3, 5, 45, true, "Strawberry");
        UpsertCrop("watermelon", "Dưa Hấu", "Dưa hấu mát lành ngày hè.",
            new float[] { 90, 110, 110, 90 }, 1, 2, 75, false, "Watermelon");

        // ==================== TIER 4: VIP (very slow, very expensive) ====================
        UpsertCrop("dragon_fruit", "Thanh Long", "Thanh long đỏ rực. Thu hoạch nhiều lần.",
            new float[] { 130, 160, 160, 150 }, 1, 2, 120, true, "DragonFruit");
        UpsertCrop("ginseng", "Nhân Sâm", "Nhân sâm quý hiếm. Giá trị cao nhất.",
            new float[] { 200, 250, 250, 200 }, 1, 1, 200, false, "Ginseng");
        UpsertCrop("rose", "Hoa Hồng", "Hoa hồng thơm ngát. Đẹp và giá trị.",
            new float[] { 160, 200, 190, 170 }, 1, 2, 150, false, "Rose");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SetupAllCrops] All 18 crops configured!");
    }

    static void UpsertCrop(string id, string displayName, string desc,
        float[] durations, int yMin, int yMax, int sellPrice, bool regrowable, string spriteFolder)
    {
        string assetPath = "Assets/Data/Crops/crop_" + id + ".asset";
        var def = AssetDatabase.LoadAssetAtPath<CropDefinition>(assetPath);

        bool isNew = (def == null);
        if (isNew)
        {
            def = ScriptableObject.CreateInstance<CropDefinition>();
        }

        def.cropId = id;
        def.cropName = displayName;
        def.description = desc;
        def.seedItemId = "seed_" + id;
        def.harvestItemId = "crop_" + id;
        def.stageDurations = durations;
        def.requiresWater = true;
        def.witherTime = 0f;
        def.yieldMin = yMin;
        def.yieldMax = yMax;
        def.regrowable = regrowable;
        def.regrowToStage = GrowthStage.Growing;
        def.sellPrice = sellPrice;

        // Load stage sprites
        string[] stageNames = { "0_seed", "1_sprout", "2_growing", "3_mature", "4_harvestable" };
        def.stageSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
        {
            string spritePath = "Assets/Sprites/Crops/" + spriteFolder + "/" + id + "_" + stageNames[i] + ".png";
            def.stageSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        }

        if (isNew)
        {
            AssetDatabase.CreateAsset(def, assetPath);
            Debug.Log("[SetupAllCrops] Created: " + assetPath);
        }
        else
        {
            EditorUtility.SetDirty(def);
            Debug.Log("[SetupAllCrops] Updated: " + assetPath);
        }
    }

    // ==================== ITEM DEFINITIONS ====================

    [MenuItem("Tools/Setup All Seed Items (Tier System)")]
    public static void SetupItems()
    {
        // Tier 1: Common
        UpsertItem("seed_bean", "Hạt Đậu", "Đậu xanh dễ trồng.", 8, 2, "bean");
        UpsertItem("seed_corn", "Hạt Ngô", "Ngô ngọt dễ trồng.", 10, 2, "corn");
        UpsertItem("seed_wheat", "Hạt Lúa Mì", "Lúa mì vàng óng.", 10, 2, "wheat");
        UpsertItem("seed_carrot", "Hạt Cà Rốt", "Cà rốt tươi giòn.", 12, 3, "carrot");
        UpsertItem("seed_potato", "Hạt Khoai Tây", "Khoai tây bùi ngon.", 12, 3, "potato");
        UpsertItem("seed_onion", "Hạt Hành Tây", "Hành tây thơm.", 15, 3, "onion");

        // Tier 2: Uncommon
        UpsertItem("seed_cucumber", "Hạt Dưa Chuột", "Dưa chuột mát lành.", 22, 5, "cucumber");
        UpsertItem("seed_tomato", "Hạt Cà Chua", "Cà chua chín mọng.", 25, 6, "tomato");
        UpsertItem("seed_cabbage", "Hạt Bắp Cải", "Bắp cải xanh tươi.", 28, 7, "cabbage");
        UpsertItem("seed_chili", "Hạt Ớt", "Ớt cay nồng.", 30, 7, "chili");
        UpsertItem("seed_eggplant", "Hạt Cà Tím", "Cà tím bổ dưỡng.", 32, 8, "eggplant");

        // Tier 3: Rare
        UpsertItem("seed_garlic", "Hạt Tỏi", "Tỏi thơm gia vị quý.", 45, 10, "garlic");
        UpsertItem("seed_pumpkin", "Hạt Bí Đỏ", "Bí đỏ to giá cao.", 50, 12, "pumpkin");
        UpsertItem("seed_strawberry", "Hạt Dâu Tây", "Dâu tây ngọt ngào.", 60, 15, "strawberry");
        UpsertItem("seed_watermelon", "Hạt Dưa Hấu", "Dưa hấu mát ngày hè.", 55, 13, "watermelon");

        // Tier 4: VIP
        UpsertItem("seed_dragon_fruit", "Hạt Thanh Long", "Thanh long đỏ rực.", 100, 25, "dragon_fruit");
        UpsertItem("seed_ginseng", "Hạt Nhân Sâm", "Nhân sâm quý hiếm.", 150, 35, "ginseng");
        UpsertItem("seed_rose", "Hạt Hoa Hồng", "Hoa hồng thơm ngát.", 120, 30, "rose");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SetupAllCrops] All 18 seed items configured!");
    }

    static void UpsertItem(string itemId, string displayName, string desc,
        int buyPrice, int sellPrice, string cropId)
    {
        string assetPath = "Assets/Data/Items/" + itemId + ".asset";
        var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(assetPath);

        bool isNew = (item == null);
        if (isNew)
        {
            item = ScriptableObject.CreateInstance<ItemDefinition>();
        }

        item.itemId = itemId;
        item.itemName = displayName;
        item.description = desc;
        item.itemType = ItemType.Seed;
        item.buyPrice = buyPrice;
        item.sellPrice = sellPrice;
        item.cropId = cropId;
        item.stackable = true;
        item.maxStack = 99;

        // Try to load seed icon
        string iconName = "Seed_" + cropId.Substring(0, 1).ToUpper() + cropId.Substring(1);
        string iconPath = "Assets/UI/" + iconName + ".png";
        item.icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

        if (isNew)
        {
            AssetDatabase.CreateAsset(item, assetPath);
            Debug.Log("[SetupAllCrops] Created item: " + assetPath);
        }
        else
        {
            EditorUtility.SetDirty(item);
            Debug.Log("[SetupAllCrops] Updated item: " + assetPath);
        }
    }

    // ==================== SHOP CATALOG ====================

    [MenuItem("Tools/Update Shop Catalog (All Seeds)")]
    public static void UpdateShop()
    {
        string catalogPath = "Assets/Data/Shop/GeneralStore.asset";
        var catalog = AssetDatabase.LoadAssetAtPath<ShopCatalog>(catalogPath);
        if (catalog == null)
        {
            Debug.LogError("[SetupAllCrops] ShopCatalog not found at: " + catalogPath);
            return;
        }

        // Collect all seed items
        string[] seedIds = {
            // Tier 1
            "seed_bean", "seed_corn", "seed_wheat", "seed_carrot", "seed_potato", "seed_onion",
            // Tier 2
            "seed_cucumber", "seed_tomato", "seed_cabbage", "seed_chili", "seed_eggplant",
            // Tier 3
            "seed_garlic", "seed_pumpkin", "seed_strawberry", "seed_watermelon",
            // Tier 4
            "seed_dragon_fruit", "seed_ginseng", "seed_rose"
        };

        var entries = new System.Collections.Generic.List<ShopEntry>();
        foreach (string sid in seedIds)
        {
            string itemPath = "Assets/Data/Items/" + sid + ".asset";
            var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(itemPath);
            if (item == null)
            {
                Debug.LogWarning("[SetupAllCrops] Item not found: " + itemPath);
                continue;
            }

            var entry = new ShopEntry();
            entry.item = item;
            entry.buyPrice = item.buyPrice;
            entry.sellPrice = item.sellPrice;
            entry.available = true;
            entry.maxStock = -1; // unlimited
            entries.Add(entry);
        }

        catalog.entries = entries.ToArray();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        Debug.Log("[SetupAllCrops] Shop updated with " + entries.Count + " seed entries!");
    }
}
