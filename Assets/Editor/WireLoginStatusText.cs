using UnityEditor;
using UnityEngine;
using TMPro;

/// <summary>
/// Creates a status text element in the LoginScene and wires it to FirebaseLoginManager.
/// </summary>
public class WireLoginStatusText
{
    [MenuItem("Tools/Wire Login Status Text")]
    public static void Wire()
    {
        var loginManager = Object.FindObjectOfType<FirebaseLoginManager>();
        if (loginManager == null)
        {
            Debug.LogError("WireLoginStatusText: FirebaseLoginManager not found! Open LoginScene first.");
            return;
        }

        var so = new SerializedObject(loginManager);
        var statusProp = so.FindProperty("statusText");

        // Check if already wired
        if (statusProp != null && statusProp.objectReferenceValue != null)
        {
            Debug.Log("WireLoginStatusText: statusText already assigned.");
            return;
        }

        // Find existing status text or create one under the login form
        Transform loginForm = loginManager.loginForm?.transform;
        if (loginForm == null)
        {
            Debug.LogError("WireLoginStatusText: loginForm reference is null!");
            return;
        }

        // Look for existing text named StatusText
        var existing = loginForm.Find("StatusText");
        TMP_Text tmpText = null;

        if (existing != null)
        {
            tmpText = existing.GetComponent<TMP_Text>();
        }

        if (tmpText == null)
        {
            // Create new StatusText at bottom of login form
            var statusGO = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusGO.transform.SetParent(loginForm, false);

            var rt = statusGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, 0f);
            rt.anchorMax = new Vector2(0.95f, 0.12f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            tmpText = statusGO.GetComponent<TextMeshProUGUI>();
            tmpText.text = "";
            tmpText.fontSize = 16;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;

            Debug.Log("WireLoginStatusText: Created StatusText under loginForm");
        }

        // Wire to FirebaseLoginManager
        statusProp.objectReferenceValue = tmpText;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(loginManager);

        Debug.Log("WireLoginStatusText: Done! Save the scene to persist.");
    }
}
