using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Tworzy mobilne UI dla P2 oraz dodaje AttackVisualizer do P1 i P2.
/// Menu: Tools -> Battle Scene Setup -> Setup Mobile UI + Visualizer
/// </summary>
public static class MobileUISetup
{
    [MenuItem("Tools/Battle Scene Setup/Setup Mobile UI + Attack Visualizer")]
    public static void Setup()
    {
        AddAttackVisualizer("P1");
        AddAttackVisualizer("P2");
        CreateMobileCanvas();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Gotowe!",
            "Dodano:\n" +
            "• AttackVisualizer do P1 i P2\n" +
            "• Canvas z przyciskami dla P2\n\n" +
            "W Inspektorze na Canvas (MobileUI):\n" +
            "  przypisz pole 'Player2' → obiekt P2\n\n" +
            "Przyciski dla P2:\n" +
            "← Lewo  | Prawo →  | ▲ Skok\n" +
            "⚡ High Attack | 🦵 Low Attack",
            "OK");
    }

    // ─────────────────────────────────────────────────────────────
    // ATTACK VISUALIZER
    // ─────────────────────────────────────────────────────────────
    static void AddAttackVisualizer(string playerName)
    {
        GameObject go = GameObject.Find(playerName);
        if (go == null) { Debug.LogWarning($"[MobileUISetup] Nie znaleziono {playerName} w scenie."); return; }

        if (go.GetComponent<AttackVisualizer>() == null)
            go.AddComponent<AttackVisualizer>();

        EditorUtility.SetDirty(go);
        Debug.Log($"[MobileUISetup] ✅ AttackVisualizer dodany do {playerName}");
    }

    // ─────────────────────────────────────────────────────────────
    // CANVAS
    // ─────────────────────────────────────────────────────────────
    static void CreateMobileCanvas()
    {
        // Usuń stary Canvas mobilny, jeśli istnieje
        GameObject old = GameObject.Find("MobileUI_Canvas");
        if (old != null) Object.DestroyImmediate(old);

        // EventSystem (wymagany przez UI)
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem",
                typeof(EventSystem),
                typeof(StandaloneInputModule));
        }

        // Canvas
        GameObject canvasGO = new GameObject("MobileUI_Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // MobileUI skrypt
        MobileUI mobileUI = canvasGO.AddComponent<MobileUI>();

        // Przypisz P2 automatycznie
        GameObject p2go = GameObject.Find("P2");
        if (p2go != null)
            mobileUI.player2 = p2go.GetComponent<FighterController>();

        // ── PRZYCISKI P2 (prawa dolna część ekranu) ──
        //    Lewo, Prawo, Skok  |  HighAttack, LowAttack

        // Lewo
        MakeButton(canvasGO, "Btn_Left", "◄",
            new Vector2(100, 80), new Vector2(120, 120),
            new Color(0.2f, 0.2f, 0.8f, 0.7f),
            mobileUI, "OnLeftDown", "OnLeftUp");

        // Prawo
        MakeButton(canvasGO, "Btn_Right", "►",
            new Vector2(240, 80), new Vector2(120, 120),
            new Color(0.2f, 0.2f, 0.8f, 0.7f),
            mobileUI, "OnRightDown", "OnRightUp");

        // Skok (wyżej)
        MakeButton(canvasGO, "Btn_Jump", "▲",
            new Vector2(170, 200), new Vector2(120, 120),
            new Color(0.2f, 0.7f, 0.2f, 0.7f),
            mobileUI, "OnJump", null);

        // High Attack
        MakeButton(canvasGO, "Btn_HighAttack", "HI\nATAK",
            new Vector2(-240, 80), new Vector2(140, 120),
            new Color(0.8f, 0.3f, 0.1f, 0.7f),
            mobileUI, "OnHighAttack", null);

        // Low Attack
        MakeButton(canvasGO, "Btn_LowAttack", "LO\nATAK",
            new Vector2(-100, 80), new Vector2(140, 120),
            new Color(0.6f, 0.1f, 0.7f, 0.7f),
            mobileUI, "OnLowAttack", null);

        EditorUtility.SetDirty(canvasGO);
        Debug.Log("[MobileUISetup] ✅ MobileUI Canvas utworzony");
    }

    // ─────────────────────────────────────────────────────────────
    // HELPER – tworzy jeden przycisk z Event Triggerami
    // ─────────────────────────────────────────────────────────────
    static void MakeButton(GameObject canvas, string name, string label,
        Vector2 anchoredPos, Vector2 size, Color color,
        MobileUI mobileUI, string downMethod, string upMethod)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(canvas.transform, false);

        // RectTransform – kotwiczymy do prawego dolnego rogu
        RectTransform rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchoredPos.x < 0 ? 1f : 0f, 0f);
        rt.anchorMax = new Vector2(anchoredPos.x < 0 ? 1f : 0f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.sizeDelta = size;
        rt.anchoredPosition = new Vector2(
            anchoredPos.x < 0 ? anchoredPos.x : anchoredPos.x,
            anchoredPos.y);

        // Tło przycisku
        Image img = btnGO.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = true;

        // Tekst
        GameObject textGO = new GameObject("Label");
        textGO.transform.SetParent(btnGO.transform, false);
        RectTransform trt = textGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        Text txt = textGO.AddComponent<Text>();
        txt.text = label;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = Color.white;
        txt.fontSize = 28;
        txt.fontStyle = FontStyle.Bold;
        txt.raycastTarget = false;

        // Event Trigger – PointerDown
        EventTrigger trigger = btnGO.AddComponent<EventTrigger>();

        AddTriggerEntry(trigger, EventTriggerType.PointerDown, mobileUI, downMethod);
        if (upMethod != null)
            AddTriggerEntry(trigger, EventTriggerType.PointerUp, mobileUI, upMethod);
    }

    static void AddTriggerEntry(EventTrigger trigger, EventTriggerType type,
        MobileUI mobileUI, string methodName)
    {
        if (string.IsNullOrEmpty(methodName)) return;

        var entry = new EventTrigger.Entry { eventID = type };

        // UnityAction podpięta przez reflection
        var method = typeof(MobileUI).GetMethod(methodName);
        if (method == null)
        {
            Debug.LogWarning($"[MobileUISetup] Metoda {methodName} nie znaleziona w MobileUI");
            return;
        }

        UnityEngine.Events.UnityAction action = (UnityEngine.Events.UnityAction)
            System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), mobileUI, method);
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }
}

