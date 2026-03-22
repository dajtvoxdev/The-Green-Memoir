using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AssignCropDefinitions
{
    [MenuItem("Tools/Assign All CropDefinitions to CropGrowthManager")]
    public static void Assign()
    {
        // Find CropGrowthManager in scene
        var mgr = Object.FindFirstObjectByType<CropGrowthManager>();
        if (mgr == null)
        {
            Debug.LogError("[AssignCropDefs] CropGrowthManager not found in scene!");
            return;
        }

        // Load all CropDefinition assets from Assets/Data/Crops/
        string[] guids = AssetDatabase.FindAssets("t:CropDefinition", new[] { "Assets/Data/Crops" });
        var defs = new List<CropDefinition>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<CropDefinition>(path);
            if (def != null)
            {
                // Skip duplicate definitions (prefer crop_ prefixed assets)
                bool isDuplicate = false;
                foreach (var existing in defs)
                {
                    if (existing.cropId == def.cropId)
                    {
                        isDuplicate = true;
                        break;
                    }
                }
                if (!isDuplicate)
                {
                    defs.Add(def);
                    Debug.Log("[AssignCropDefs] Found: " + path + " (cropId: " + def.cropId + ")");
                }
                else
                {
                    Debug.LogWarning("[AssignCropDefs] Skipping duplicate: " + path + " (cropId: " + def.cropId + ")");
                }
            }
        }

        mgr.cropDefinitions = defs.ToArray();
        EditorUtility.SetDirty(mgr);

        // Also mark the scene dirty so changes persist
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mgr.gameObject.scene);

        Debug.Log("[AssignCropDefs] Assigned " + defs.Count + " CropDefinitions to CropGrowthManager!");
    }
}
