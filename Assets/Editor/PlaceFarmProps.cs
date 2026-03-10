using UnityEngine;
using UnityEditor;

/// <summary>
/// Places Vietnamese countryside farm props on the rectangular grid map.
/// Positions aligned with PaintFarmLayout.cs zones:
///
///   Pond:      x[-18,-11] y[-12,-6]    (bottom-left)
///   FarmSoil:  4 rectangular plots      (bottom row + top-left garden)
///   Rice:      x[8,19] y[4,12]         (top-right)
///   Path/Yard: main x[-18,18] y[-3,-1], yard x[-8,5] y[7,13]
///   Canal:     borders around rice paddy
/// </summary>
public static class PlaceFarmProps
{
    const string OBJ = "Assets/Sprites/Objects";
    const string CUL = "Assets/Sprites/Objects/Cultural";

    [MenuItem("Tools/Place Vietnamese Farm Props")]
    public static void PlaceProps()
    {
        // Remove old props
        var old = GameObject.Find("VietnamFarmProps");
        if (old != null)
        {
            Undo.DestroyObjectImmediate(old);
            Debug.Log("PlaceFarmProps: Removed old props");
        }

        var root = new GameObject("VietnamFarmProps");
        Undo.RegisterCreatedObjectUndo(root, "Place Farm Props");
        int count = 0;

        // ═══════════════════════════════════════════════════
        // HOUSES — In the yard area x[-8,5] y[7,13]
        // ═══════════════════════════════════════════════════
        count += Place(root, "House_Main",     $"{OBJ}/house_red_roof.png", -2f, 10.5f, 2);
        count += Place(root, "House_Side",     $"{OBJ}/house_red_roof.png",  3f, 10.5f, 2);

        // ═══════════════════════════════════════════════════
        // TREES — Bamboo and banana around the map
        // ═══════════════════════════════════════════════════
        // Bamboo behind houses (north)
        count += Place(root, "Bamboo_N1",      $"{OBJ}/bamboo_cluster.png", -4f, 13f, 2);
        count += Place(root, "Bamboo_N2",      $"{OBJ}/bamboo_cluster.png",  1f, 13f, 2);
        // Bamboo near top-left garden
        count += Place(root, "Bamboo_NW",      $"{OBJ}/bamboo_cluster.png",-15f,  8f, 2);
        // Bamboo near rice paddy
        count += Place(root, "Bamboo_E",       $"{OBJ}/bamboo_cluster.png", 19f, 10f, 2);

        // Banana trees scattered
        count += Place(root, "Banana_W1",      $"{OBJ}/banana_tree.png",   -10f, -1f, 2);
        count += Place(root, "Banana_W2",      $"{OBJ}/banana_tree.png",   -19f, -6f, 2);
        count += Place(root, "Banana_E",       $"{OBJ}/banana_tree.png",    15f, -5f, 2);

        // ═══════════════════════════════════════════════════
        // ANIMALS — Buffalo near rice, chickens in yard
        // ═══════════════════════════════════════════════════
        // Water buffalo near rice paddy entrance
        count += Place(root, "Buffalo",        $"{OBJ}/water_buffalo.png",  12f,  2f, 3);

        // Chickens in the yard
        count += Place(root, "Chicken_1",      $"{OBJ}/chicken.png",        -3f,  8f, 3);
        count += Place(root, "Chicken_2",      $"{OBJ}/chicken.png",        -1f,  7.5f, 3);
        count += Place(root, "Chicken_3",      $"{OBJ}/chicken.png",        -4f,  7.5f, 3);

        // Dog near house entrance
        count += Place(root, "Dog",            $"{OBJ}/dog.png",            -6f,  8f, 3);

        // Cat on the porch
        count += Place(root, "Cat",            $"{OBJ}/cat.png",             4f,  9f, 3);

        // ═══════════════════════════════════════════════════
        // CULTURAL OBJECTS
        // ═══════════════════════════════════════════════════
        // Well (giếng khơi) — in the yard
        count += Place(root, "Well",           $"{CUL}/gieng_khoi.png",     -6f, 11f, 3);

        // Clay jars (chum vại) — near houses
        count += Place(root, "Jar_1",          $"{CUL}/chum_vai.png",       -3f,  9f, 3);
        count += Place(root, "Jar_2",          $"{CUL}/chum_vai.png",        1f,  9f, 3);

        // Lanterns (đèn lồng) — along the main path
        count += Place(root, "Lantern_1",      $"{CUL}/den_long.png",      -12f, -1f, 4);
        count += Place(root, "Lantern_2",      $"{CUL}/den_long.png",        0f, -1f, 4);
        count += Place(root, "Lantern_3",      $"{CUL}/den_long.png",       10f, -1f, 4);

        // Incense rack (giàn phơi hương) — in the yard
        count += Place(root, "IncenseRack",    $"{CUL}/gian_phoi_huong.png", -7f,  9f, 3);

        // Kumquat trees (cây quất) — flanking house entrance
        count += Place(root, "Kumquat_L",      $"{CUL}/cay_quat.png",       -4f,  8.5f, 3);
        count += Place(root, "Kumquat_R",      $"{CUL}/cay_quat.png",        0f,  8.5f, 3);

        // Fan plants — near the pond
        count += Place(root, "FanPlant_1",     $"{CUL}/cay_quat.png",      -14f, -6f, 3);
        count += Place(root, "FanPlant_2",     $"{CUL}/cay_quat.png",      -12f, -10f, 3);

        // Mark scene dirty
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log($"PlaceFarmProps: Placed {count} props! Save scene with Ctrl+S.");
    }

    static int Place(GameObject parent, string name, string path, float x, float y, int sortOrder)
    {
        // Try loading as Sprite directly, then fall back to Texture2D sub-assets
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite s) { sprite = s; break; }
            }
        }
        if (sprite == null)
        {
            Debug.LogWarning($"PlaceFarmProps: No sprite at '{path}'");
            return 0;
        }

        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = new Vector3(x, y, -0.5f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = sortOrder;

        return 1;
    }
}
