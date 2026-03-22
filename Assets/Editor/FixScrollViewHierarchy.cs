using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using PolyAndCode.UI;

public static class FixScrollViewHierarchy
{
    [MenuItem("Tools/Fix ScrollView Hierarchy")]
    public static void Run()
    {
        var scrollGO = GameObject.Find("RecyclableScroll");
        if (scrollGO == null) { Debug.LogError("[FixScrollView] RecyclableScroll not found!"); return; }

        var scrollRect = scrollGO.GetComponent<RecyclableScrollRect>();
        if (scrollRect == null) { Debug.LogError("[FixScrollView] RecyclableScrollRect not found!"); return; }

        // Remove any broken Viewport from prior attempt
        var oldViewport = scrollGO.transform.Find("Viewport");
        if (oldViewport != null) Object.DestroyImmediate(oldViewport.gameObject);

        // Create Viewport with RectTransform explicitly in constructor
        var viewportGO = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportGO.transform.SetParent(scrollGO.transform, false);

        var vpRT = viewportGO.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = Vector2.zero;
        vpRT.pivot = new Vector2(0f, 1f);

        var vpImg = viewportGO.GetComponent<Image>();
        vpImg.color = new Color(1, 1, 1, 0.01f);
        vpImg.raycastTarget = false;

        var mask = viewportGO.GetComponent<Mask>();
        mask.showMaskGraphic = false;

        // Move Content under Viewport
        var contentT = scrollGO.transform.Find("Content");
        if (contentT != null)
        {
            contentT.SetParent(viewportGO.transform, false);
            var contentRT = contentT.GetComponent<RectTransform>();
            if (contentRT != null)
            {
                contentRT.anchorMin = new Vector2(0, 1);
                contentRT.anchorMax = new Vector2(1, 1);
                contentRT.pivot = new Vector2(0.5f, 1f);
                contentRT.anchoredPosition = Vector2.zero;
                contentRT.sizeDelta = new Vector2(0, 0);
            }
        }

        // Wire m_Viewport
        var so = new SerializedObject(scrollRect);
        so.FindProperty("m_Viewport").objectReferenceValue = vpRT;
        so.ApplyModifiedProperties();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("[FixScrollView] Done. Viewport wired, Content moved under Viewport.");
    }
}
