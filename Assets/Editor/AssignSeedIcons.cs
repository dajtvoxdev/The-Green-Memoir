using UnityEngine;
using UnityEditor;

public class AssignSeedIcons
{
    [MenuItem("Tools/Moonlit Garden/Assign Seed Icons")]
    public static void AssignIcons()
    {
        // Carrot
        AssignIcon("Assets/Data/Items/seed_carrot.asset", "Assets/UI/Seed_Carrot.png");
        
        // Pumpkin
        AssignIcon("Assets/Data/Items/seed_pumpkin.asset", "Assets/UI/Seed_Pumpkin.png");
        
        // Cabbage
        AssignIcon("Assets/Data/Items/seed_cabbage.asset", "Assets/UI/Seed_Cabbage.png");
        
        // Cucumber
        AssignIcon("Assets/Data/Items/seed_cucumber.asset", "Assets/UI/Seed_Cucumber.png");
        
        // Onion
        AssignIcon("Assets/Data/Items/seed_onion.asset", "Assets/UI/Seed_Onion.png");
        
        // Potato
        AssignIcon("Assets/Data/Items/seed_potato.asset", "Assets/UI/Seed_Potato.png");
        
        AssetDatabase.SaveAssets();
        Debug.Log("Seed icons assigned!");
    }
    
    private static void AssignIcon(string itemPath, string iconPath)
    {
        ItemDefinition item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(itemPath);
        Sprite icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
        
        if (item != null && icon != null)
        {
            item.icon = icon;
            EditorUtility.SetDirty(item);
            Debug.Log($"Assigned icon to {item.itemName}");
        }
        else
        {
            Debug.LogWarning($"Could not assign icon: {itemPath} or {iconPath} not found");
        }
    }
}
