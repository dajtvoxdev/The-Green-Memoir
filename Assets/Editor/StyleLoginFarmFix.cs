using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class StyleLoginFarmFix
{
    [MenuItem("Tools/StyleLoginFarmFix")]
    public static void Apply()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        Font cherryFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/UI/Fonts/Cherry_Bomb_One/CherryBombOne-Regular.ttf");
        Font arialFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // UI Root
        Transform bgTr = canvas.transform.Find("Background");
        if (bgTr != null)
        {
            Image bgImg = bgTr.GetComponent<Image>();
            if (bgImg != null)
            {
                Undo.RecordObject(bgImg, "Color");
                bgImg.color = new Color32(25, 35, 55, 255); // Night sky
            }

            Transform loginForm = bgTr.Find("LoginForm");
            if (loginForm != null)
            {
                Image formImg = loginForm.GetComponent<Image>();
                if (formImg != null)
                {
                    Undo.RecordObject(formImg, "Color");
                    formImg.color = new Color32(50, 35, 25, 220); // Dark wood container
                }

                // 1. Header
                Transform header = loginForm.Find("Header");
                if (header != null)
                {
                    Image headerImg = header.GetComponent<Image>();
                    if (headerImg != null)
                    {
                        Undo.RecordObject(headerImg, "Color");
                        headerImg.color = new Color(0, 0, 0, 0); // Transparent
                    }

                    Transform logo = header.Find("GameLogo");
                    if (logo != null) logo.gameObject.SetActive(false); // Hide old leaf logo

                    Transform gameTitle = header.Find("GameTitle");
                    if (gameTitle != null)
                    {
                        Text t = gameTitle.GetComponent<Text>();
                        Undo.RecordObject(t, "Text Change");
                        t.text = "Moonlit Garden";
                        t.font = cherryFont;
                        t.fontSize = 64;
                        t.color = new Color32(130, 210, 50, 255);
                        t.alignment = TextAnchor.MiddleCenter;

                        AddOutline(gameTitle.gameObject, new Color32(30, 60, 10, 255), new Vector2(3, -3));
                        AddShadow(gameTitle.gameObject, new Color32(10, 25, 5, 200), new Vector2(4, -5));
                        PrefabUtility.RecordPrefabInstancePropertyModifications(t);
                    }
                }

                // 2. LoginWithAccount
                StylePanel(loginForm.Find("LoginWithAccount"), cherryFont, arialFont);
                // 3. RegisterForm
                StylePanel(loginForm.Find("RegisterForm"), cherryFont, arialFont);
                // 4. LoginFast
                StyleLoginFast(loginForm.Find("LoginFast"), cherryFont, arialFont);
            }
        }

        EditorUtility.SetDirty(canvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("StyleLoginFarmFix done!");
    }

    static void StylePanel(Transform panel, Font titleFont, Font regularFont)
    {
        if (panel == null) return;
        Image img = panel.GetComponent<Image>();
        if (img != null)
        {
            Undo.RecordObject(img, "Color");
            img.color = new Color32(75, 55, 40, 250); // Wooden brown panel
            PrefabUtility.RecordPrefabInstancePropertyModifications(img);
        }

        foreach (Text t in panel.GetComponentsInChildren<Text>(true))
        {
            Undo.RecordObject(t, "Text");
            t.font = regularFont; // Use arial for vietnamese
            t.fontStyle = FontStyle.Bold;

            if (t.gameObject.name == "Login" || t.gameObject.name == "Register")
            {
                t.font = titleFont;
                t.fontSize = 36;
                t.color = new Color32(255, 220, 120, 255); // Yellow wood
            }
            else if (t.gameObject.name.Contains("Notify"))
            {
                t.color = new Color32(255, 100, 80, 255);
            }
            else if (t.gameObject.name == "ForgotPassword")
            {
                t.color = new Color32(180, 220, 255, 200);
            }
            else if (t.transform.parent != null && t.transform.parent.GetComponent<Button>() != null)
            {
                t.color = Color.white;
            }
            else
            {
                t.color = new Color32(240, 230, 210, 255); // Input box text
            }
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);
        }

        foreach (InputField input in panel.GetComponentsInChildren<InputField>(true))
        {
            Image inputImg = input.GetComponent<Image>();
            if (inputImg != null)
            {
                Undo.RecordObject(inputImg, "Color");
                inputImg.color = new Color32(40, 30, 22, 230); // Very dark brown for input
                PrefabUtility.RecordPrefabInstancePropertyModifications(inputImg);
            }
        }

        foreach (Button btn in panel.GetComponentsInChildren<Button>(true))
        {
            Undo.RecordObject(btn, "Color");
            var cb = btn.colors;
            if (btn.gameObject.name.Contains("Login") || btn.gameObject.name.Contains("Register"))
            {
                cb.normalColor = new Color32(80, 150, 50, 255); // Green primary button
            }
            else
            {
                cb.normalColor = new Color32(100, 70, 45, 255); // Brown secondary
            }
            btn.colors = cb;
            PrefabUtility.RecordPrefabInstancePropertyModifications(btn);
        }

        Transform line = panel.Find("LeftLine");
        if (line != null)
        {
            Image lImg = line.GetComponent<Image>();
            if (lImg != null)
            {
                Undo.RecordObject(lImg, "Color");
                lImg.color = new Color32(100, 80, 50, 150);
                PrefabUtility.RecordPrefabInstancePropertyModifications(lImg);
            }
        }
    }

    static void StyleLoginFast(Transform panel, Font titleFont, Font regularFont)
    {
        if (panel == null) return;
        Image img = panel.GetComponent<Image>();
        if (img != null)
        {
            Undo.RecordObject(img, "Color");
            img.color = new Color32(65, 45, 30, 250); // Wooden dark
            PrefabUtility.RecordPrefabInstancePropertyModifications(img);
        }

        foreach (Text t in panel.GetComponentsInChildren<Text>(true))
        {
            Undo.RecordObject(t, "Text");
            t.font = regularFont;
            t.fontStyle = FontStyle.Bold;
            if (t.gameObject.name == "OrLoginWith") t.color = new Color32(200, 190, 170, 200);
            else t.color = Color.white;
            PrefabUtility.RecordPrefabInstancePropertyModifications(t);
        }

        foreach (Button btn in panel.GetComponentsInChildren<Button>(true))
        {
            Undo.RecordObject(btn, "Color");
            var cb = btn.colors;
            if (btn.gameObject.name.Contains("Facebook")) cb.normalColor = new Color32(60, 90, 150, 255);
            else if (btn.gameObject.name.Contains("Google")) cb.normalColor = new Color32(180, 60, 40, 255);
            else if (btn.gameObject.name.Contains("PlayNow")) cb.normalColor = new Color32(80, 150, 50, 255);
            btn.colors = cb;
            PrefabUtility.RecordPrefabInstancePropertyModifications(btn);
        }
    }

    static void AddOutline(GameObject go, Color c, Vector2 dist)
    {
        Outline o = go.GetComponent<Outline>();
        if (o == null) o = Undo.AddComponent<Outline>(go);
        Undo.RecordObject(o, "Outline");
        o.effectColor = c;
        o.effectDistance = dist;
    }
    static void AddShadow(GameObject go, Color c, Vector2 dist)
    {
        Shadow s = go.GetComponent<Shadow>();
        if (s == null) s = Undo.AddComponent<Shadow>(go);
        Undo.RecordObject(s, "Shadow");
        s.effectColor = c;
        s.effectDistance = dist;
    }
}
