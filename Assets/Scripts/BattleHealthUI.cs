using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wyswietla zdrowie graczy w procentach w dolnych rogach ekranu.
/// Lewy dolny rog: Gracz 1 (P1)
/// Prawy dolny rog: Gracz 2 (P2)
/// </summary>
public class BattleHealthUI : MonoBehaviour
{
    public static BattleHealthUI Instance;

    [Header("Gracze")]
    public FighterController player1;
    public FighterController player2;

    [Header("Wyswietlacze tekstu (opcjonalne - jesli puste, utworza sie automatycznie)")]
    public Text p1HealthText;
    public Text p2HealthText;
    public TMP_Text p1HealthTMP;
    public TMP_Text p2HealthTMP;

    [Header("Format")]
    public string p1Prefix = "P1 HP: ";
    public string p2Prefix = "P2 HP: ";
    public Color normalColor = new Color(0.2f, 0.9f, 0.2f, 1f); // zielony
    public Color warningColor = new Color(0.95f, 0.85f, 0.1f, 1f); // zolty
    public Color dangerColor = new Color(0.95f, 0.2f, 0.2f, 1f); // czerwony

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        FindPlayersIfNull();
        CreateUIElementsIfMissing();
        SubscribeToHealthEvents();
        UpdateAllDisplay();
    }

    void OnDestroy()
    {
        UnsubscribeFromHealthEvents();
    }

    void FindPlayersIfNull()
    {
        if (player1 == null || player2 == null)
        {
            var p1Obj = GameObject.Find("P1");
            if (p1Obj != null && player1 == null) player1 = p1Obj.GetComponent<FighterController>();

            var p2Obj = GameObject.Find("P2");
            if (p2Obj != null && player2 == null) player2 = p2Obj.GetComponent<FighterController>();

            if (player1 == null || player2 == null)
            {
                var fighters = FindObjectsByType<FighterController>(FindObjectsSortMode.None);
                if (fighters.Length > 0 && player1 == null) player1 = fighters[0];
                if (fighters.Length > 1 && player2 == null) player2 = fighters[1];
            }
        }
    }

    void SubscribeToHealthEvents()
    {
        if (player1 != null && player1.Health != null)
            player1.Health.OnHealthChanged += OnP1HealthChanged;

        if (player2 != null && player2.Health != null)
            player2.Health.OnHealthChanged += OnP2HealthChanged;
    }

    void UnsubscribeFromHealthEvents()
    {
        if (player1 != null && player1.Health != null)
            player1.Health.OnHealthChanged -= OnP1HealthChanged;

        if (player2 != null && player2.Health != null)
            player2.Health.OnHealthChanged -= OnP2HealthChanged;
    }

    void OnP1HealthChanged(int current, int max)
    {
        UpdatePlayerUI(current, max, p1Prefix, p1HealthText, p1HealthTMP);
    }

    void OnP2HealthChanged(int current, int max)
    {
        UpdatePlayerUI(current, max, p2Prefix, p2HealthText, p2HealthTMP);
    }

    public void UpdateAllDisplay()
    {
        if (player1 != null && player1.Health != null)
            UpdatePlayerUI(player1.Health.currentHealth, player1.Health.maxHealth, p1Prefix, p1HealthText, p1HealthTMP);

        if (player2 != null && player2.Health != null)
            UpdatePlayerUI(player2.Health.currentHealth, player2.Health.maxHealth, p2Prefix, p2HealthText, p2HealthTMP);
    }

    void UpdatePlayerUI(int current, int max, string prefix, Text uText, TMP_Text tmpText)
    {
        int percent = (max > 0) ? Mathf.Clamp(Mathf.RoundToInt((float)current / max * 100f), 0, 100) : 0;
        string textStr = $"{prefix}{percent}%";

        Color c = (percent > 50) ? normalColor : (percent > 25) ? warningColor : dangerColor;

        if (uText != null)
        {
            uText.text = textStr;
            uText.color = c;
        }

        if (tmpText != null)
        {
            tmpText.text = textStr;
            tmpText.color = c;
        }
    }

    void CreateUIElementsIfMissing()
    {
        // Sprawdz czy mamy juz podpiete teksty
        if (p1HealthText != null || p1HealthTMP != null) return;

        // Szukaj istniejacego Canvasa w scenie
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            var canvasGO = new GameObject("HealthCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // P1 Text - lewy dolny rog
        if (p1HealthText == null && p1HealthTMP == null)
        {
            var p1GO = new GameObject("HealthPercent_P1");
            p1GO.transform.SetParent(canvas.transform, false);

            var rt = p1GO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0f, 0f);
            rt.anchoredPosition = new Vector2(30f, 25f);
            rt.sizeDelta = new Vector2(250f, 60f);

            var txt = p1GO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 32;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.LowerLeft;
            txt.text = "P1 HP: 100%";
            txt.color = normalColor;

            var outline = p1GO.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2f, -2f);

            p1HealthText = txt;
        }

        // P2 Text - prawy dolny rog
        if (p2HealthText == null && p2HealthTMP == null)
        {
            var p2GO = new GameObject("HealthPercent_P2");
            p2GO.transform.SetParent(canvas.transform, false);

            var rt = p2GO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-30f, 25f);
            rt.sizeDelta = new Vector2(250f, 60f);

            var txt = p2GO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 32;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.LowerRight;
            txt.text = "P2 HP: 100%";
            txt.color = normalColor;

            var outline = p2GO.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2f, -2f);

            p2HealthText = txt;
        }
    }
}

