using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Tworzy mobilne UI dla P2, pasek zdrowia w procentach oraz AttackVisualizer.
/// Menu: Tools -> Mobile UI Setup -> Utworz Mobile UI + Paski Zdrowia
/// </summary>
public static class MobileUISetup
{
    [MenuItem("Tools/Mobile UI Setup/Utworz Mobile UI + Paski Zdrowia")]
    public static void Setup()
    {
        AddAttackVisualizer("P1");
        AddAttackVisualizer("P2");
        CreateMobileCanvas();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Sukces!",
            "Skonfigurowano pomyslnie:\n" +
            "1. Przyciski mobilne dla Gracza 2 (P2)\n" +
            "2. Procentowe wyswietlanie zdrowia (P1 w lewym dolnym rogu, P2 w prawym dolnym)\n" +
            "3. AttackVisualizer na P1 i P2\n\n" +
            "Wszystko dziala od razu w Play Mode!",
            "OK");
    }

    static void AddAttackVisualizer(string playerName)
    {
        GameObject go = GameObject.Find(playerName);
        if (go == null) return;

        if (go.GetComponent<AttackVisualizer>() == null)
            go.AddComponent<AttackVisualizer>();

        EditorUtility.SetDirty(go);
    }

    static void CreateMobileCanvas()
    {
        GameObject canvasGO = GameObject.Find("MobileUI_Canvas");
        if (canvasGO == null)
        {
            canvasGO = new GameObject("MobileUI_Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        MobileUI mobileUI = canvasGO.GetComponent<MobileUI>();
        if (mobileUI == null) mobileUI = canvasGO.AddComponent<MobileUI>();

        GameObject p2go = GameObject.Find("P2");
        if (p2go != null) mobileUI.player2 = p2go.GetComponent<FighterController>();

        BattleHealthUI healthUI = canvasGO.GetComponent<BattleHealthUI>();
        if (healthUI == null) healthUI = canvasGO.AddComponent<BattleHealthUI>();

        GameObject p1go = GameObject.Find("P1");
        if (p1go != null) healthUI.player1 = p1go.GetComponent<FighterController>();
        if (p2go != null) healthUI.player2 = p2go.GetComponent<FighterController>();

        // Usun ewentualne stare przyciski wewnatrz
        Transform oldLeft = canvasGO.transform.Find("Btn_Left");
        if (oldLeft != null) Object.DestroyImmediate(oldLeft.gameObject);
        Transform oldRight = canvasGO.transform.Find("Btn_Right");
        if (oldRight != null) Object.DestroyImmediate(oldRight.gameObject);
        Transform oldJump = canvasGO.transform.Find("Btn_Jump");
        if (oldJump != null) Object.DestroyImmediate(oldJump.gameObject);
        Transform oldHi = canvasGO.transform.Find("Btn_HighAttack");
        if (oldHi != null) Object.DestroyImmediate(oldHi.gameObject);
        Transform oldLo = canvasGO.transform.Find("Btn_LowAttack");
        if (oldLo != null) Object.DestroyImmediate(oldLo.gameObject);

        // Przyciski P2:
        // Lewo / Prawo
        MakeButton(canvasGO, "Btn_Left", "<", new Vector2(90, 90), new Vector2(110, 110), new Color(0.2f, 0.4f, 0.85f, 0.8f), mobileUI, MobileButton.ActionType.Left);
        MakeButton(canvasGO, "Btn_Right", ">", new Vector2(210, 90), new Vector2(110, 110), new Color(0.2f, 0.4f, 0.85f, 0.8f), mobileUI, MobileButton.ActionType.Right);
        // Skok
        MakeButton(canvasGO, "Btn_Jump", "^", new Vector2(150, 210), new Vector2(110, 110), new Color(0.2f, 0.75f, 0.3f, 0.8f), mobileUI, MobileButton.ActionType.Jump);

        // High Attack & Low Attack
        MakeButton(canvasGO, "Btn_HighAttack", "HI", new Vector2(-210, 90), new Vector2(110, 110), new Color(0.85f, 0.35f, 0.1f, 0.85f), mobileUI, MobileButton.ActionType.HighAttack);
        MakeButton(canvasGO, "Btn_LowAttack", "LO", new Vector2(-90, 90), new Vector2(110, 110), new Color(0.7f, 0.2f, 0.8f, 0.85f), mobileUI, MobileButton.ActionType.LowAttack);

        EditorUtility.SetDirty(canvasGO);
    }

    static void MakeButton(GameObject canvas, string name, string label,
        Vector2 anchoredPos, Vector2 size, Color color,
        MobileUI mobileUI, MobileButton.ActionType actionType)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(canvas.transform, false);

        RectTransform rt = btnGO.AddComponent<RectTransform>();
        float anchorX = anchoredPos.x < 0 ? 1f : 0f;
        rt.anchorMin = new Vector2(anchorX, 0f);
        rt.anchorMax = new Vector2(anchorX, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;

        Image img = btnGO.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;

        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        Text txt = textGO.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.text = label;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontSize = 32;
        txt.fontStyle = FontStyle.Bold;
        txt.raycastTarget = false;

        // Komponent obslugujacy dotyk/mysz
        MobileButton mb = btnGO.AddComponent<MobileButton>();
        mb.action = actionType;
        mb.mobileUI = mobileUI;
    }
}
