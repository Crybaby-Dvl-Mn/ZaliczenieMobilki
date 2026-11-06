using System.Collections;
using UnityEngine;

/// <summary>
/// Wizualizacja atakow:
/// - Startup: postac miga na zolto
/// - Hitbox: kwadrat pojawia sie w miejscu hitboxa
/// - Hit: zielony flash
/// - ReceiveHit: czerwony flash
/// </summary>
[RequireComponent(typeof(AttackExecutor))]
public class AttackVisualizer : MonoBehaviour
{
    [Header("Kolory")]
    public Color startupColor   = new Color(1f, 1f, 0f, 0.8f);   // zolty
    public Color hitActiveColor = new Color(1f, 0.3f, 0f, 0.8f); // pomaranczowy
    public Color hitLandedColor = new Color(0f, 1f, 0f, 0.9f);   // zielony
    public Color receivedHitColor = new Color(1f, 0f, 0f, 0.8f); // czerwony

    public Sprite hitboxSprite;

    private SpriteRenderer _body;
    private Color _originalColor;
    private GameObject _hitboxQuad;
    private SpriteRenderer _hitboxRenderer;
    private Coroutine _flashCoroutine;

    void Awake()
    {
        _body = GetComponent<SpriteRenderer>();
        if (_body != null)
        {
            _originalColor = _body.color;
        }

        _hitboxQuad = new GameObject("_HitboxVisual");
        _hitboxQuad.transform.SetParent(null);
        _hitboxRenderer = _hitboxQuad.AddComponent<SpriteRenderer>();
        _hitboxRenderer.sortingOrder = 100;

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

    public void PlayStartup(float duration)
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashBody(startupColor, duration));
    }

    public void ShowHitbox(AttackData data, FighterController owner)
    {
        var executor = owner.GetComponent<AttackExecutor>();
        if (executor == null) return;

        Vector2 center = executor.GetHitboxCenter(owner, data);

        _hitboxQuad.transform.position = new Vector3(center.x, center.y, -0.5f);
        _hitboxQuad.transform.localScale = new Vector3(data.hitboxSize.x, data.hitboxSize.y, 1f);

        Color c = (data.attackType == AttackType.High)
            ? new Color(1f, 0.4f, 0.1f, 0.45f)
            : new Color(0.2f, 0.5f, 1f, 0.45f);

        _hitboxRenderer.color = c;
        _hitboxQuad.SetActive(true);
    }

    public void HideHitbox()
    {
        if (_hitboxQuad != null)
            _hitboxQuad.SetActive(false);
    }

    public void PlayHit()
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashBody(hitLandedColor, 0.08f));
    }

    public void PlayReceiveHit()
    {
        if (_flashCoroutine != null) StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashBody(receivedHitColor, 0.15f));
    }

    private IEnumerator FlashBody(Color flashColor, float duration)
    {
        if (_body == null) yield break;

        float half = duration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            _body.color = Color.Lerp(_originalColor, flashColor, t / half);
            t += Time.deltaTime;
            yield return null;
        }
        _body.color = flashColor;

        t = 0f;
        while (t < half)
        {
            _body.color = Color.Lerp(flashColor, _originalColor, t / half);
            t += Time.deltaTime;
            yield return null;
        }
        _body.color = _originalColor;
    }
}
