using UnityEngine;

/// <summary>
/// Growth stages for a crop. Maps to TilemapDetail.growthStage (0-4).
/// </summary>
public enum GrowthStage
{
    Seed = 0,
    Sprout = 1,
    Growing = 2,
    Mature = 3,
    Harvestable = 4
}

/// <summary>
/// ScriptableObject defining a crop's properties and growth behavior.
/// Create via: Assets > Create > MoonlitGarden > Crops > CropDefinition
/// </summary>
[CreateAssetMenu(fileName = "NewCrop", menuName = "MoonlitGarden/Crops/CropDefinition")]
public class CropDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique crop ID (e.g., 'crop_tomato')")]
    public string cropId;

    [Tooltip("Display name")]
    public string cropName;

    [TextArea]
    [Tooltip("Description shown in UI")]
    public string description;

    [Header("Linked Items")]
    [Tooltip("ItemDefinition ID for the seed that plants this crop")]
    public string seedItemId;

    [Tooltip("ItemDefinition ID for the harvested product")]
    public string harvestItemId;

    [Header("Growth Timing")]
    [Tooltip("Time in seconds for each growth stage transition")]
    public float[] stageDurations = new float[4] { 30f, 60f, 90f, 60f };

    [Tooltip("Does this crop require watering to advance stages?")]
    public bool requiresWater = true;

    [Tooltip("Time in seconds before crop withers without water (0 = never withers)")]
    public float witherTime = 0f;

    [Header("Harvest")]
    [Tooltip("Minimum yield quantity when harvested")]
    public int yieldMin = 1;

    [Tooltip("Maximum yield quantity when harvested")]
    public int yieldMax = 3;

    [Tooltip("Can this crop be harvested multiple times?")]
    public bool regrowable = false;

    [Tooltip("If regrowable, which stage to return to after harvest")]
    public GrowthStage regrowToStage = GrowthStage.Growing;

    [Header("Visuals")]
    [Tooltip("Sprites for each growth stage (Seed, Sprout, Growing, Mature, Harvestable)")]
    public Sprite[] stageSprites = new Sprite[5];

    [Header("Economy")]
    [Tooltip("Sell price per unit of harvested crop")]
    public int sellPrice = 10;

    /// <summary>
    /// Total time from seed to harvestable (sum of all stage durations).
    /// </summary>
    public float TotalGrowTime
    {
        get
        {
            float total = 0f;
            foreach (float d in stageDurations) total += d;
            return total;
        }
    }

    /// <summary>
    /// Gets the sprite for the given growth stage.
    /// Returns null if stage is out of range or sprites not assigned.
    /// </summary>
    public Sprite GetStageSprite(GrowthStage stage)
    {
        int idx = (int)stage;
        if (stageSprites != null && idx >= 0 && idx < stageSprites.Length)
        {
            return stageSprites[idx];
        }
        return null;
    }

    /// <summary>
    /// Gets the cumulative time needed to reach a given stage from Seed.
    /// </summary>
    public float GetTimeToReachStage(GrowthStage stage)
    {
        int targetIdx = (int)stage;
        float time = 0f;
        for (int i = 0; i < targetIdx && i < stageDurations.Length; i++)
        {
            time += stageDurations[i];
        }
        return time;
    }

    /// <summary>
    /// Calculates current growth stage based on elapsed time since planting.
    /// </summary>
    public GrowthStage CalculateStageFromElapsed(float elapsedSeconds)
    {
        float accumulated = 0f;
        for (int i = 0; i < stageDurations.Length; i++)
        {
            accumulated += stageDurations[i];
            if (elapsedSeconds < accumulated)
            {
                return (GrowthStage)i;
            }
        }
        return GrowthStage.Harvestable;
    }

    /// <summary>
    /// Rolls a random yield between yieldMin and yieldMax (inclusive).
    /// </summary>
    public int RollYield()
    {
        return Random.Range(yieldMin, yieldMax + 1);
    }
}
