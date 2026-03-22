using UnityEngine;
using UnityEditor;

public class CreateCropDefinitions
{
    [MenuItem("Tools/Create Missing CropDefinitions")]
    public static void Create()
    {
        CreateCrop("carrot", "Cà Rốt", "Cà rốt tươi giòn.", "seed_carrot", "crop_carrot",
            new float[] { 25, 50, 75, 50 }, 2, 4, 25, false);

        CreateCrop("onion", "Hành Tây", "Hành tây thơm ngon.", "seed_onion", "crop_onion",
            new float[] { 30, 55, 80, 55 }, 2, 4, 30, false);

        CreateCrop("cucumber", "Dưa Chuột", "Dưa chuột mát lành. Cho thu hoạch nhiều lần.", "seed_cucumber", "crop_cucumber",
            new float[] { 20, 45, 70, 45 }, 1, 3, 20, true);

        CreateCrop("cabbage", "Bắp Cải", "Bắp cải xanh tươi. Giá cao.", "seed_cabbage", "crop_cabbage",
            new float[] { 35, 65, 100, 65 }, 1, 2, 55, false);

        CreateCrop("pumpkin", "Bí Đỏ", "Bí đỏ to tròn. Giá bán rất cao.", "seed_pumpkin", "crop_pumpkin",
            new float[] { 40, 70, 110, 70 }, 1, 2, 75, false);

        CreateCrop("potato", "Khoai Tây", "Khoai tây bùi ngon. Thu hoạch nhiều củ.", "seed_potato", "crop_potato",
            new float[] { 30, 55, 85, 55 }, 3, 6, 20, false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateCropDefinitions] Done creating crop definitions!");
    }

    static void CreateCrop(string id, string displayName, string desc,
        string seedItemId, string harvestItemId,
        float[] durations, int yMin, int yMax, int sellPrice, bool regrowable)
    {
        string assetPath = "Assets/Data/Crops/crop_" + id + ".asset";

        // Skip if already exists
        var existing = AssetDatabase.LoadAssetAtPath<CropDefinition>(assetPath);
        if (existing != null)
        {
            Debug.Log("[CreateCropDefinitions] Already exists: " + assetPath);
            return;
        }

        var def = ScriptableObject.CreateInstance<CropDefinition>();
        def.cropId = id;
        def.cropName = displayName;
        def.description = desc;
        def.seedItemId = seedItemId;
        def.harvestItemId = harvestItemId;
        def.stageDurations = durations;
        def.requiresWater = true;
        def.witherTime = 0f;
        def.yieldMin = yMin;
        def.yieldMax = yMax;
        def.regrowable = regrowable;
        def.regrowToStage = GrowthStage.Growing;
        def.sellPrice = sellPrice;

        // Load stage sprites
        string cropCap = id.Substring(0, 1).ToUpper() + id.Substring(1);
        string[] stageNames = { "0_seed", "1_sprout", "2_growing", "3_mature", "4_harvestable" };
        def.stageSprites = new Sprite[5];
        for (int i = 0; i < 5; i++)
        {
            string spritePath = "Assets/Sprites/Crops/" + cropCap + "/" + id + "_" + stageNames[i] + ".png";
            def.stageSprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (def.stageSprites[i] == null)
            {
                Debug.LogWarning("[CreateCropDefinitions] Sprite not found: " + spritePath);
            }
        }

        AssetDatabase.CreateAsset(def, assetPath);
        Debug.Log("[CreateCropDefinitions] Created: " + assetPath);
    }
}
