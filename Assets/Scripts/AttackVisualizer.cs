using System.Collections;
using UnityEngine;

/// <summary>
/// Wizualizacja ataków na czas gdy nie ma animacji:
/// – Startup: postać miga na żółto
/// – Hitbox: kolorowy kwadrat pojawia się w miejscu hitboxa
/// – Hit: krótki zielony flash na napastniku
/// – ReceiveHit: postać miga na czerwono
/// Dodaj ten komponent na obiekty P1 i P2.
/// </summary>
[RequireComponent(typeof(AttackExecutor))]
public class AttackVisualizer : MonoBehaviour
{
    [Header("Kolory")]
    public Color startupColor   = new Color(1f, 1f, 0f, 0.8f);   // zolty = startup
    public Color hitActiveColor = new Color(1f, 0.3f, 0f, 0.8f); // pomaranczowy = aktywny hitbox
    public Color hitLandedColor = new Color(0f, 1f, 0f, 0.9f);   // zielony = trafienie
    public Color receivedHitColor = new Color(1f, 0f, 0f, 0.8f); // czerwony = otrzymany cios

    [Header("Hitbox sprite (zostaw pusty – uzyje automatycznie kwadratu)")]
    public Sprite hitboxSprite;

    // --- referencje wewnętrzne ---
    private SpriteRenderer _body;
    private Color _originalColor;
    private GameObject _hitboxQuad;
    private SpriteRenderer _hitboxRenderer;
    private Coroutine _flashCoroutine;

    void Awake()
    {
        _body = GetComponent<SpriteRenderer>();
        if (_body == null)
        {
            // Jeżeli postać to kostka (MeshRenderer) – nie ma SpriteRenderer, działamy w trybie tylko hitboxa
        }
        else
        {
            _originalColor = _body.color;
        }

        // Stwórz kwadrat hitboxa jako dziecko
        _hitboxQuad = new GameObject("_HitboxVisual");
        _hitboxQuad.transform.SetParent(null); // niezależny od postaci
        _hitboxRenderer = _hitboxQuad.AddComponent<SpriteRenderer>();
        _hitboxRenderer.sortingOrder = 100;

        // Domyślny sprite Unity (biały kwadrat)
        _hitboxRenderer.sprite = hitboxSprite != null
            ? hitboxSprite
            : Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        _hitboxQuad.SetActive(false);
    }

    void OnDestroy()
    {
        if (_hitboxQuad != null)
            Destroy(_hitboxQuad);
    }

    // --- STARTUP FLASH (żółty migot postaci) ---
    public void PlayStartup(float duration)
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashBody(startupColor, duration));
    }

    // --- POKAŻ HITBOX ---
    public void ShowHitbox(AttackData data, FighterController owner)
    {
        var executor = owner.GetComponent<AttackExecutor>();
        if (executor == null) return;

        Vector2 center = executor.GetHitboxCenter(owner, data);

        _hitboxQuad.transform.position = new Vector3(center.x, center.y, -0.5f);
        _hitboxQuad.transform.localScale = new Vector3(data.hitboxSize.x, data.hitboxSize.y, 1f);

        // Kolor zależny od typu ataku
        Color c = (data.attackType == AttackType.High)
            ? new Color(1f, 0.4f, 0.1f, 0.45f)   // pomarańczowy = ręka
            : new Color(0.2f, 0.5f, 1f, 0.45f);   // niebieski = noga

        _hitboxRenderer.color = c;
        _hitboxQuad.SetActive(true);
    }

    // --- UKRYJ HITBOX ---
    public void HideHitbox()
    {
        if (_hitboxQuad != null)
            _hitboxQuad.SetActive(false);
    }

    // --- TRAFIENIE WYKRYTE (flash zielony) ---
    public void PlayHit()
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashBody(hitLandedColor, 0.08f));
    }

    // --- OTRZYMANIE CIOSU (flash czerwony) – wywołaj z FighterController.ReceiveHit ---
    public void PlayReceiveHit()
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashBody(receivedHitColor, 0.15f));
    }

    // --- FLASH CIAŁA ---
    private IEnumerator FlashBody(Color flashColor, float duration)
    {
        if (_body == null) yield break;

        float half = duration * 0.5f;
        float t = 0f;

        // rozjaśnij do flashColor
        while (t < half)
        {
            _body.color = Color.Lerp(_originalColor, flashColor, t / half);
            t += Time.deltaTime;
            yield return null;
        }
        _body.color = flashColor;

        t = 0f;
        // wróć do oryginalnego
        while (t < half)
        {
            _body.color = Color.Lerp(flashColor, _originalColor, t / half);
            t += Time.deltaTime;
            yield return null;
        }
        _body.color = _originalColor;
    }
}

