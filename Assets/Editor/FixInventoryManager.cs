using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using PolyAndCode.UI;

public class FixInventoryManager
{
    [MenuItem("Tools/Moonlit Garden/Fix InventoryManager Reference")]
    public static void FixInventoryScrollRect()
    {
        // Find Canvas and InventoryHUD
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }
        
        Transform inventoryHUD = canvas.transform.Find("InventoryHUD");
        if (inventoryHUD == null)
        {
            Debug.LogError("InventoryHUD not found!");
            return;
        }
        
        // Find RecyclableInventoryManager
        RecyclableInventoryManager inventoryManager = Object.FindFirstObjectByType<RecyclableInventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogError("RecyclableInventoryManager not found!");
            return;
        }
        
        // Check if InventoryHUD already has RecyclableScrollRect
        RecyclableScrollRect scrollRect = inventoryHUD.GetComponentInChildren<RecyclableScrollRect>();
        
        if (scrollRect == null)
        {
            // Create new GameObject with RecyclableScrollRect
            GameObject scrollObj = new GameObject("RecyclableScroll", typeof(RectTransform));
            scrollObj.transform.SetParent(inventoryHUD, false);
            
            RectTransform rect = scrollObj.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Add components
            scrollObj.AddComponent<CanvasRenderer>();
            scrollObj.AddComponent<Image>();
            scrollRect = scrollObj.AddComponent<RecyclableScrollRect>();
            
            // Set up ScrollRect properties
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Elastic;
            
            Debug.Log("Created RecyclableScrollRect in InventoryHUD");
        }
        else
        {
            Debug.Log("Found existing RecyclableScrollRect in InventoryHUD");
        }
        
        // Assign to InventoryManager using SerializedObject
        SerializedObject so = new SerializedObject(inventoryManager);
        SerializedProperty prop = so.FindProperty("_recyclableScrollRect");
        if (prop != null)
        {
            prop.objectReferenceValue = scrollRect;
            so.ApplyModifiedProperties();
            Debug.Log("Assigned RecyclableScrollRect to InventoryManager");
        }
        else
        {
            Debug.LogError("Could not find _recyclableScrollRect property!");
        }
    }
}
